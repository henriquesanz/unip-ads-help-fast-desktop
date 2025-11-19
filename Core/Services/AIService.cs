using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Models.Entities.JavaApi;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Collections.Generic;

namespace HelpFastDesktop.Core.Services;

public class AIService : IAIService
{
    private readonly ApplicationDbContext _context;
    private readonly IGoogleDriveService _googleDriveService;
    private readonly IOpenAIService _openAIService;
    private readonly SemaphoreSlim _systemPromptLock = new(1, 1);
    private string? _cachedSystemPrompt;
    private const string ProcedimentosFileId = "11QDEi5sSNZb99EPmL5HZqe0-ASecHgWO4yXZC4Cl1FU";

    public AIService(ApplicationDbContext context, IGoogleDriveService googleDriveService, IOpenAIService openAIService)
    {
        _context = context;
        _googleDriveService = googleDriveService;
        _openAIService = openAIService;
    }

    #region Chat Inteligente

    public async Task<ChatResponse?> ProcessarMensagemChatAsync(int usuarioId, string mensagem, int? chamadoId = null)
    {
        try
        {
            // Buscar contexto do usuário
            var contexto = await ConstruirContextoUsuarioAsync(usuarioId);

            var systemPrompt = await ObterSystemPromptAsync(contexto);

            // Buscar histórico de mensagens do banco se houver ChamadoId
            var historicoMensagens = new List<ChatMessage>();
            if (chamadoId.HasValue)
            {
                historicoMensagens = await BuscarHistoricoChatAsync(chamadoId.Value);
            }

            // Enviar mensagem com histórico para OpenAI
            string respostaTexto;
            if (historicoMensagens.Count > 0)
            {
                respostaTexto = await _openAIService.EnviarPerguntaComHistoricoAsync(mensagem, systemPrompt, historicoMensagens);
            }
            else
            {
                respostaTexto = await _openAIService.EnviarPerguntaAsync(mensagem, systemPrompt);
            }

            // Salvar mensagem do usuário na tabela Chats
            if (chamadoId.HasValue)
            {
                // Verificar se é mensagem de contexto inicial ou mensagem normal do usuário
                string tipoMensagem = mensagem.StartsWith("Estou analisando o chamado") ? "IA_Context" : "IA_User";
                // Para mensagens do usuário, o destinatário é o próprio usuário (conversa com IA)
                // O banco não permite NULL em DestinatarioId
                await SalvarMensagemChatAsync(chamadoId.Value, usuarioId, usuarioId, mensagem, tipoMensagem);
            }

            // Salvar resposta da IA na tabela Chats
            if (chamadoId.HasValue)
            {
                // Para mensagens da IA, usaremos RemetenteId = usuarioId mas Tipo = "IA_Assistant"
                // O DestinatarioId também será o usuarioId (usuário recebe a resposta da IA)
                await SalvarMensagemChatAsync(chamadoId.Value, usuarioId, usuarioId, respostaTexto, "IA_Assistant");
            }

            var response = new ChatResponse
            {
                Resposta = respostaTexto,
                EscalarParaHumano = false
            };

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar mensagem com OpenAI para usuário {usuarioId}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Erro interno: {ex.InnerException.Message}");
            }

            return new ChatResponse
            {
                Resposta = "Desculpe, ocorreu um erro ao processar sua mensagem com o serviço de IA. Tente novamente mais tarde.",
                EscalarParaHumano = true
            };
        }
    }

    private async Task<List<ChatMessage>> BuscarHistoricoChatAsync(int chamadoId)
    {
        try
        {
            var chats = await _context.Chats
                .Where(c => c.ChamadoId == chamadoId && 
                           (c.Tipo == "IA_User" || c.Tipo == "IA_Assistant" || c.Tipo == "IA_Context"))
                .OrderBy(c => c.DataEnvio)
                .ToListAsync();

            var historico = new List<ChatMessage>();
            foreach (var chat in chats)
            {
                if (chat.Tipo == "IA_User" || chat.Tipo == "IA_Context")
                {
                    historico.Add(new ChatMessage
                    {
                        Role = "user",
                        Content = chat.Mensagem
                    });
                }
                else if (chat.Tipo == "IA_Assistant")
                {
                    historico.Add(new ChatMessage
                    {
                        Role = "assistant",
                        Content = chat.Mensagem
                    });
                }
            }

            return historico;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar histórico de chat para chamado {chamadoId}: {ex.Message}");
            return new List<ChatMessage>();
        }
    }

    private async Task SalvarMensagemChatAsync(int chamadoId, int remetenteId, int? destinatarioId, string mensagem, string tipo)
    {
        try
        {
            // O banco não permite NULL em DestinatarioId, então usamos o remetenteId como fallback
            int destinatarioIdFinal = destinatarioId ?? remetenteId;

            var chat = new Chat
            {
                ChamadoId = chamadoId,
                RemetenteId = remetenteId,
                DestinatarioId = destinatarioIdFinal,
                Mensagem = mensagem,
                Tipo = tipo,
                DataEnvio = DateTime.Now
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar mensagem de chat: {ex.Message}");
            // Não lançar exceção para não interromper o fluxo
        }
    }

    public async Task SalvarMensagemChatSimplesAsync(int chamadoId, int remetenteId, string mensagem, string tipo)
    {
        // O banco não permite NULL em DestinatarioId, então usamos o remetenteId como destinatário
        await SalvarMensagemChatAsync(chamadoId, remetenteId, remetenteId, mensagem, tipo);
    }

    #endregion

    #region Categorização Automática

    public async Task<CategorizacaoResponse?> CategorizarChamadoAsync(Chamado chamado)
    {
        try
        {
            if (!await IsCategorizacaoAtivaAsync())
            {
                Console.WriteLine("Categorização automática desabilitada");
                return new CategorizacaoResponse();
            }

            var descricao = (chamado.Titulo + " " + chamado.Motivo).ToLowerInvariant();

            var response = new CategorizacaoResponse();

            if (descricao.Contains("senha") || descricao.Contains("login") || descricao.Contains("acesso"))
            {
                response.Categoria = "Acesso";
                response.Subcategoria = "Credenciais";
                response.Confianca = 0.7m;
            }
            else if (descricao.Contains("internet") || descricao.Contains("rede") || descricao.Contains("wifi"))
            {
                response.Categoria = "Rede";
                response.Subcategoria = "Conectividade";
                response.Confianca = 0.6m;
            }
            else if (descricao.Contains("impressora") || descricao.Contains("scanner") || descricao.Contains("hardware"))
            {
                response.Categoria = "Hardware";
                response.Subcategoria = "Periféricos";
                response.Confianca = 0.55m;
            }
            else if (descricao.Contains("erro") || descricao.Contains("bug") || descricao.Contains("falha"))
            {
                response.Categoria = "Aplicação";
                response.Subcategoria = "Instabilidade";
                response.Confianca = 0.5m;
            }
            else
            {
                response.Categoria = "Geral";
                response.Subcategoria = "Outros";
                response.Confianca = 0.4m;
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao categorizar chamado {chamado.Id}: {ex.Message}");
            return new CategorizacaoResponse();
        }
    }

    public async Task<bool> ValidarCategorizacaoAsync(int chamadoId, string categoriaOriginal, string categoriaCorrigida)
    {
        try
        {
            var coerente = string.Equals(categoriaOriginal, categoriaCorrigida, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(categoriaOriginal);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao validar categorização do chamado {chamadoId}: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Atribuição Automática

    public async Task<AtribuicaoResponse?> AtribuirChamadoAsync(int chamadoId, List<Usuario> tecnicos)
    {
        try
        {
            if (!await IsAtribuicaoAtivaAsync())
            {
                Console.WriteLine("Atribuição automática desabilitada");
                return new AtribuicaoResponse();
            }

            if (tecnicos == null || tecnicos.Count == 0)
            {
                Console.WriteLine("Nenhum técnico disponível para atribuição");
                return new AtribuicaoResponse();
            }

            var chamado = await _context.Chamados.FindAsync(chamadoId);
            if (chamado == null)
            {
                Console.WriteLine($"Chamado {chamadoId} não encontrado para atribuição");
                return new AtribuicaoResponse();
            }

            var tecnicoSelecionado = tecnicos
                .OrderBy(t => t.UltimoLogin ?? DateTime.MinValue)
                .First();

            chamado.TecnicoId = tecnicoSelecionado.Id;
            chamado.Status = "EmAndamento";
            await _context.SaveChangesAsync();

            return new AtribuicaoResponse
            {
                TecnicoId = tecnicoSelecionado.Id,
                TecnicoNome = tecnicoSelecionado.Nome,
                TecnicoEmail = tecnicoSelecionado.Email,
                Confianca = 1.0m,
                Justificativa = "Atribuição baseada na disponibilidade (último login)."
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atribuir chamado {chamadoId}: {ex.Message}");
            return new AtribuicaoResponse();
        }
    }

    #endregion

    #region Análise de Padrões

    public async Task<AnalisePadroesResponse?> AnalisarPadroesAsync(DateTime dataInicio, DateTime dataFim, string? categoria = null)
    {
        try
        {
            var chamados = await _context.Chamados
                .Where(c => c.DataAbertura >= dataInicio && c.DataAbertura <= dataFim)
                .ToListAsync();

            if (!chamados.Any())
            {
                return new AnalisePadroesResponse
                {
                    Estatisticas = new EstatisticasGerais
                    {
                        TotalChamados = 0,
                        TempoMedioResolucao = 0,
                        TaxaResolucao = 0,
                        CategoriaMaisComum = null
                    },
                    TendenciaCategorias = new List<TendenciaCategoria>(),
                    TendenciaTempo = new List<TendenciaTempo>(),
                    ProblemasRecorrentes = new List<ProblemaRecorrente>()
                };
            }

            var resolvidos = chamados.Where(c => c.DataFechamento.HasValue).ToList();
            var tempoMedioResolucao = resolvidos.Any()
                ? resolvidos
                    .Where(c => c.DataFechamento.HasValue)
                    .Average(c => (c.DataFechamento!.Value - c.DataAbertura).TotalHours)
                : 0;

            var tendenciaTempo = chamados
                .GroupBy(c => c.DataAbertura.Date)
                .Select(g => new TendenciaTempo
                {
                    Data = g.Key,
                    QuantidadeChamados = g.Count(),
                    TempoMedioResolucao = g
                        .Where(c => c.DataFechamento.HasValue)
                        .Select(c => (c.DataFechamento!.Value - c.DataAbertura).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .OrderBy(t => t.Data)
                .ToList();

            var problemasRecorrentes = chamados
                .GroupBy(c => c.Titulo)
                .Select(g => new ProblemaRecorrente
                {
                    Descricao = g.Key,
                    Frequencia = g.Count(),
                    Categoria = null
                })
                .OrderByDescending(p => p.Frequencia)
                .Take(5)
                .ToList();

            return new AnalisePadroesResponse
            {
                Estatisticas = new EstatisticasGerais
                {
                    TotalChamados = chamados.Count,
                    TempoMedioResolucao = Math.Round(tempoMedioResolucao, 2),
                    TaxaResolucao = Math.Round((double)resolvidos.Count / chamados.Count * 100, 2),
                    CategoriaMaisComum = null
                },
                TendenciaCategorias = new List<TendenciaCategoria>(),
                TendenciaTempo = tendenciaTempo,
                ProblemasRecorrentes = problemasRecorrentes
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao analisar padrões de {dataInicio} a {dataFim}: {ex.Message}");
            return new AnalisePadroesResponse();
        }
    }

    public async Task<List<string>> SugerirFAQAsync(string descricao, string? categoria = null)
    {
        try
        {
            // Buscar FAQs relevantes baseado na descrição e categoria
            var query = _context.Faqs
                .Where(f => f.Ativo)
                .AsQueryable();

            // Categoria não existe no banco, ignorando filtro

            // Busca simples por palavras-chave na descrição
            var palavrasChave = descricao.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            var faqs = await query
                .Where(f => palavrasChave.Any(p => 
                    f.Titulo.ToLower().Contains(p) || 
                    f.Descricao.ToLower().Contains(p) ||
                    (!string.IsNullOrEmpty(f.Tags) && f.Tags.ToLower().Contains(p))))
                .OrderByDescending(f => f.Utilidade)
                .ThenByDescending(f => f.Visualizacoes)
                .Take(5)
                .Select(f => f.Titulo)
                .ToListAsync();

            return faqs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao sugerir FAQ para descrição: {ex.Message}");
            return new List<string>();
        }
    }

    #endregion

    #region Configurações

    public Task<bool> IsCategorizacaoAtivaAsync()
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsAtribuicaoAtivaAsync()
    {
        return Task.FromResult(true);
    }

    #endregion

    #region Métodos Auxiliares

    private async Task<string> ObterProcedimentosAsync()
    {
        if (!string.IsNullOrEmpty(_cachedSystemPrompt))
        {
            return _cachedSystemPrompt;
        }

        await _systemPromptLock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_cachedSystemPrompt))
            {
                return _cachedSystemPrompt;
            }

            var conteudo = await _googleDriveService.LerDocumentoComoStringAsync(ProcedimentosFileId);
            _cachedSystemPrompt = conteudo;
            return conteudo;
        }
        finally
        {
            _systemPromptLock.Release();
        }
    }

    private async Task<string> ObterSystemPromptAsync(Dictionary<string, object> contextoUsuario)
    {
        var procedimentos = await ObterProcedimentosAsync();
        var builder = new StringBuilder();

        builder.AppendLine("Você é o assistente virtual HelpFast. Utilize as orientações abaixo para responder aos usuários.");
        builder.AppendLine();
        builder.AppendLine("### Procedimentos de Verificação");
        builder.AppendLine(procedimentos);
        builder.AppendLine();
        builder.AppendLine("### Contexto do Usuário");

        if (contextoUsuario.TryGetValue("nomeUsuario", out var nome))
        {
            builder.AppendLine($"- Nome: {nome}");
        }

        if (contextoUsuario.TryGetValue("emailUsuario", out var email))
        {
            builder.AppendLine($"- E-mail: {email}");
        }

        if (contextoUsuario.TryGetValue("tipoUsuario", out var tipo))
        {
            builder.AppendLine($"- Tipo de usuário: {tipo}");
        }

        if (contextoUsuario.TryGetValue("historicoInteracoes", out var historico) && historico is System.Collections.IEnumerable historicoList)
        {
            builder.AppendLine("- Interações recentes:");
            int index = 1;
            foreach (var item in historicoList)
            {
                builder.AppendLine($"  {index++}. {JsonSerializer.Serialize(item)}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Responda de forma objetiva, cordial e baseada nas orientações apresentadas. Caso a dúvida não esteja coberta, informe que vai encaminhar para suporte humano.");

        return builder.ToString();
    }

    private async Task<Dictionary<string, object>> ConstruirContextoUsuarioAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        var contexto = new Dictionary<string, object>();

        if (usuario != null)
        {
            contexto["tipoUsuario"] = usuario.TipoUsuario.ToString();
            contexto["nomeUsuario"] = usuario.Nome;
            contexto["emailUsuario"] = usuario.Email;
            contexto["historicoInteracoes"] = new List<object>();
        }

        return contexto;
    }

    #endregion
}
