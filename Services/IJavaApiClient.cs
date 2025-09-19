using HelpFastDesktop.Models;
using HelpFastDesktop.Models.JavaApi;

namespace HelpFastDesktop.Services;

public interface IJavaApiClient
{
    // Configurações
    Task<string> GetBaseUrlAsync();
    Task<int> GetTimeoutAsync();
    Task<int> GetRetryAttemptsAsync();
    Task<bool> IsCategorizacaoAtivaAsync();
    Task<bool> IsAtribuicaoAtivaAsync();

    // Processamento de Chamados
    Task<ChamadoProcessamentoResponse?> ProcessarChamadoAsync(Chamado chamado);
    Task<CategorizacaoResponse?> CategorizarChamadoAsync(Chamado chamado);
    Task<AtribuicaoResponse?> AtribuirChamadoAsync(int chamadoId, List<Usuario> tecnicos);

    // Chat com IA
    Task<ChatResponse?> EnviarMensagemChatAsync(int usuarioId, string mensagem, Dictionary<string, object> contexto);
    Task<bool> ValidarCategorizacaoAsync(int chamadoId, string categoriaOriginal, string categoriaCorrigida);

    // Análise de Padrões
    Task<AnalisePadroesResponse?> AnalisarPadroesAsync(DateTime dataInicio, DateTime dataFim, string? categoria = null);

    // Notificações
    Task<NotificacaoResponse?> EnviarNotificacaoAsync(NotificacaoRequest request);

    // Health Check
    Task<HealthCheckResponse?> VerificarSaudeAsync();
}

