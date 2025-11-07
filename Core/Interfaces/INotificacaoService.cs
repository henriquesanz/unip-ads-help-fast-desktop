using HelpFastDesktop.Core.Models.Entities;

namespace HelpFastDesktop.Core.Interfaces;

public interface INotificacaoService
{
    // Notificações básicas
    Task<Notificacao> CriarNotificacaoAsync(Notificacao notificacao);
    Task<List<Notificacao>> ListarNotificacoesUsuarioAsync(int usuarioId, bool apenasNaoLidas = false);
    Task<bool> MarcarComoLidaAsync(int notificacaoId);
    Task<bool> MarcarTodasComoLidasAsync(int usuarioId);
    Task<int> ContarNotificacoesNaoLidasAsync(int usuarioId);

    // Notificações específicas por eventos
    Task NotificarChamadoCriadoAsync(Chamado chamado);
    Task NotificarChamadoAtribuidoAsync(Chamado chamado, Usuario tecnico);
    Task NotificarChamadoResolvidoAsync(Chamado chamado);
    Task NotificarChamadoComentadoAsync(Chamado chamado, Comentario comentario);
    Task NotificarStatusAlteradoAsync(Chamado chamado, string statusAnterior);

    // Configurações de notificação
    Task<ConfiguracaoNotificacao> ObterConfiguracaoUsuarioAsync(int usuarioId);
    Task<ConfiguracaoNotificacao> AtualizarConfiguracaoAsync(ConfiguracaoNotificacao configuracao);
    Task<ConfiguracaoNotificacao> CriarConfiguracaoPadraoAsync(int usuarioId);

    // Envio de notificações
    Task<bool> EnviarNotificacaoEmailAsync(Notificacao notificacao);
    Task<bool> EnviarNotificacaoPushAsync(Notificacao notificacao);
    Task ProcessarFilaNotificacoesAsync();

    // Relatórios
    Task<List<Notificacao>> ObterEstatisticasAsync(DateTime dataInicio, DateTime dataFim);
}
