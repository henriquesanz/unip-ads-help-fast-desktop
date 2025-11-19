using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Models.Entities.JavaApi;

namespace HelpFastDesktop.Core.Interfaces;

public interface IAIService
{
    // Chat inteligente
    Task<ChatResponse?> ProcessarMensagemChatAsync(int usuarioId, string mensagem, int? chamadoId = null);
    Task SalvarMensagemChatSimplesAsync(int chamadoId, int remetenteId, string mensagem, string tipo);

    // Categorização automática
    Task<CategorizacaoResponse?> CategorizarChamadoAsync(Chamado chamado);
    Task<bool> ValidarCategorizacaoAsync(int chamadoId, string categoriaOriginal, string categoriaCorrigida);

    // Atribuição automática
    Task<AtribuicaoResponse?> AtribuirChamadoAsync(int chamadoId, List<Usuario> tecnicos);

    // Análise de padrões
    Task<AnalisePadroesResponse?> AnalisarPadroesAsync(DateTime dataInicio, DateTime dataFim, string? categoria = null);
    Task<List<string>> SugerirFAQAsync(string descricao, string? categoria = null);

    // Configurações
    Task<bool> IsCategorizacaoAtivaAsync();
    Task<bool> IsAtribuicaoAtivaAsync();
}
