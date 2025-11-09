using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Models.Entities.JavaApi;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

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

    public async Task<ChatResponse?> ProcessarMensagemChatAsync(int usuarioId, string mensagem)
    {
        try
        {
            // Buscar contexto do usuário
            var contexto = await ConstruirContextoUsuarioAsync(usuarioId);

            var systemPrompt = await ObterSystemPromptAsync(contexto);
            var respostaTexto = await _openAIService.EnviarPerguntaAsync(mensagem, systemPrompt);

            var response = new ChatResponse
            {
                Resposta = respostaTexto,
                EscalarParaHumano = false
            };

            await SalvarInteracaoIAAsync(usuarioId, "Chat", mensagem, response.Resposta, response.Categoria);

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

    public async Task<List<InteracaoIA>> ObterHistoricoChatAsync(int usuarioId, int? chamadoId = null)
    {
        var query = _context.InteracoesIA
            .Where(i => i.UsuarioId == usuarioId && i.TipoInteracao == "Chat");

        if (chamadoId.HasValue)
            query = query.Where(i => i.ChamadoId == chamadoId);

        return await query
            .OrderBy(i => i.DataInteracao)
            .ToListAsync();
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

            await SalvarInteracaoIAAsync(chamado.UsuarioId, "Categorizacao",
                $"Título: {chamado.Titulo}\nDescrição: {chamado.Descricao}",
                $"Categoria sugerida: {response.Categoria} / {response.Subcategoria}",
                response.Categoria, chamado.Id, response.Confianca);

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

            await SalvarInteracaoIAAsync(chamado.UsuarioId, "Atribuicao",
                $"Chamado {chamadoId} atribuído automaticamente",
                $"Técnico: {tecnicoSelecionado.Nome} ({tecnicoSelecionado.Email})",
                chamado.Categoria, chamadoId, 1.0m);

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

            // Histórico de interações recentes (com tratamento de erro caso a tabela não exista)
            try
            {
                var historico = await _context.InteracoesIA
                    .Where(i => i.UsuarioId == usuarioId)
                    .OrderByDescending(i => i.DataInteracao)
                    .Take(5)
                    .Select(i => new { i.Pergunta, i.Resposta })
                    .ToListAsync();

                contexto["historicoInteracoes"] = historico;
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 208) // Invalid object name
            {
                Console.WriteLine($"[AI SERVICE] AVISO: Tabela 'InteracoesIA' não existe no banco de dados.");
                Console.WriteLine($"[AI SERVICE] Execute o script SQL em: scripts/criar_tabela_interacoes_ia.sql");
                contexto["historicoInteracoes"] = new List<object>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AI SERVICE] AVISO: Não foi possível buscar histórico de interações. Erro: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[AI SERVICE] Erro interno: {ex.InnerException.Message}");
                }
                contexto["historicoInteracoes"] = new List<object>();
            }
        }

        return contexto;
    }

    private async Task SalvarInteracaoIAAsync(int usuarioId, string tipoInteracao, string? pergunta, 
        string? resposta, string? categoria = null, int? chamadoId = null, decimal? confianca = null)
    {
        try
        {
            // Verificar se a tabela existe antes de tentar salvar
            if (!await _context.Database.CanConnectAsync())
            {
                Console.WriteLine("[AI SERVICE] AVISO: Não foi possível conectar ao banco de dados para salvar interação.");
                return;
            }

            var interacao = new InteracaoIA
            {
                UsuarioId = usuarioId,
                TipoInteracao = tipoInteracao,
                Pergunta = pergunta,
                Resposta = resposta,
                Categoria = categoria,
                ChamadoId = chamadoId,
                Confianca = confianca,
                DataInteracao = DateTime.Now
            };

            _context.InteracoesIA.Add(interacao);
            await _context.SaveChangesAsync();
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 208) // Invalid object name
        {
            Console.WriteLine($"[AI SERVICE] AVISO: Tabela 'InteracoesIA' não existe no banco de dados. Interação não foi salva.");
            Console.WriteLine($"[AI SERVICE] Para criar a tabela, execute o script SQL da documentação ESTRUTURA_DADOS_BANCO.md");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AI SERVICE] Erro ao salvar interação IA para usuário {usuarioId}: {ex.Message}");
            Console.WriteLine($"[AI SERVICE] Tipo de exceção: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[AI SERVICE] Erro interno: {ex.InnerException.Message}");
            }
        }
    }

    #endregion
}
