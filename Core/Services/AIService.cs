using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Models.Entities.JavaApi;
using HelpFastDesktop.Core.Interfaces;

namespace HelpFastDesktop.Core.Services;

public class AIService : IAIService
{
    private readonly IJavaApiClient _javaApiClient;
    private readonly ApplicationDbContext _context;

    public AIService(IJavaApiClient javaApiClient, ApplicationDbContext context)
    {
        _javaApiClient = javaApiClient;
        _context = context;
    }

    #region Chat Inteligente

    public async Task<ChatResponse?> ProcessarMensagemChatAsync(int usuarioId, string mensagem)
    {
        try
        {
            // Buscar contexto do usuário
            var contexto = await ConstruirContextoUsuarioAsync(usuarioId);

            // Enviar para API Java
            var response = await _javaApiClient.EnviarMensagemChatAsync(usuarioId, mensagem, contexto);

            // Salvar interação no banco
            await SalvarInteracaoIAAsync(usuarioId, "Chat", mensagem, response?.Resposta, response?.Categoria);

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar mensagem de chat para usuário {usuarioId}: {ex.Message}");
            return new ChatResponse 
            { 
                Resposta = "Desculpe, ocorreu um erro ao processar sua mensagem. Tente novamente mais tarde.",
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

            var response = await _javaApiClient.CategorizarChamadoAsync(chamado);

            if (response != null && !string.IsNullOrEmpty(response.Categoria))
            {
                // Categoria e Subcategoria não existem no banco, apenas logando
                // chamado.Categoria e chamado.Subcategoria são read-only

                // Salvar interação
                await SalvarInteracaoIAAsync(chamado.UsuarioId, "Categorizacao", 
                    $"Título: {chamado.Titulo}\nDescrição: {chamado.Descricao}", 
                    $"Categoria: {response.Categoria}, Subcategoria: {response.Subcategoria}",
                    response.Categoria, chamado.Id, response.Confianca);
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
            var sucesso = await _javaApiClient.ValidarCategorizacaoAsync(chamadoId, categoriaOriginal, categoriaCorrigida);

            if (sucesso)
            {
                // Atualizar chamado
                var chamado = await _context.Chamados.FindAsync(chamadoId);
                if (chamado != null)
                {
                    // Categoria não existe no banco, apenas logando
                    await _context.SaveChangesAsync();
                }
            }

            return sucesso;
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

            var response = await _javaApiClient.AtribuirChamadoAsync(chamadoId, tecnicos);

            if (response != null && response.TecnicoId > 0)
            {
                // Atualizar chamado
                var chamado = await _context.Chamados.FindAsync(chamadoId);
                if (chamado != null)
                {
                    chamado.TecnicoId = response.TecnicoId;
                    chamado.Status = "EmAndamento";
                    await _context.SaveChangesAsync();

                    // Salvar interação
                    await SalvarInteracaoIAAsync(chamado.UsuarioId, "Atribuicao", 
                        $"Chamado {chamadoId} atribuído", 
                        $"Técnico: {response.TecnicoNome} ({response.TecnicoEmail})",
                        chamado.Categoria, chamadoId, response.Confianca);
                }
            }

            return response;
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
            return await _javaApiClient.AnalisarPadroesAsync(dataInicio, dataFim, categoria);
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
                    f.Tags.ToLower().Contains(p)))
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

    public async Task<bool> IsCategorizacaoAtivaAsync()
    {
        return await _javaApiClient.IsCategorizacaoAtivaAsync();
    }

    public async Task<bool> IsAtribuicaoAtivaAsync()
    {
        return await _javaApiClient.IsAtribuicaoAtivaAsync();
    }

    #endregion

    #region Métodos Auxiliares

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
