using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Models.Entities.JavaApi;

namespace HelpFastDesktop.Core.Services;

public class NotificacaoService : INotificacaoService
{
    private readonly ApplicationDbContext _context;
    private readonly IJavaApiClient _javaApiClient;

    public NotificacaoService(ApplicationDbContext context, IJavaApiClient javaApiClient)
    {
        _context = context;
        _javaApiClient = javaApiClient;
    }

    #region Notificações Básicas

    public async Task<Notificacao> CriarNotificacaoAsync(Notificacao notificacao)
    {
        try
        {
            notificacao.DataEnvio = DateTime.Now;
            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();

            // Enviar por canais configurados
            await EnviarPorCanaisAsync(notificacao);

            return notificacao;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar notificação para usuário {notificacao.UsuarioId}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Notificacao>> ListarNotificacoesUsuarioAsync(int usuarioId, bool apenasNaoLidas = false)
    {
        var query = _context.Notificacoes
            .Where(n => n.UsuarioId == usuarioId);

        if (apenasNaoLidas)
            query = query.Where(n => !n.Lida);

        return await query
            .OrderByDescending(n => n.DataEnvio)
            .ToListAsync();
    }

    public async Task<bool> MarcarComoLidaAsync(int notificacaoId)
    {
        try
        {
            var notificacao = await _context.Notificacoes.FindAsync(notificacaoId);
            if (notificacao == null) return false;

            notificacao.Lida = true;
            notificacao.DataLeitura = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao marcar notificação {NotificacaoId} como lida: ex");
            return false;
        }
    }

    public async Task<bool> MarcarTodasComoLidasAsync(int usuarioId)
    {
        try
        {
            var notificacoes = await _context.Notificacoes
                .Where(n => n.UsuarioId == usuarioId && !n.Lida)
                .ToListAsync();

            foreach (var notificacao in notificacoes)
            {
                notificacao.Lida = true;
                notificacao.DataLeitura = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao marcar todas as notificações como lidas para usuário {UsuarioId}: ex");
            return false;
        }
    }

    public async Task<int> ContarNotificacoesNaoLidasAsync(int usuarioId)
    {
        return await _context.Notificacoes
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Lida);
    }

    #endregion

    #region Notificações Específicas por Eventos

    public async Task NotificarChamadoCriadoAsync(Chamado chamado)
    {
        try
        {
            var notificacao = new Notificacao
            {
                UsuarioId = chamado.UsuarioId,
                Tipo = "chamado_criado",
                Titulo = "Chamado Criado",
                Mensagem = $"Seu chamado '{chamado.Titulo}' foi criado com sucesso e recebeu o ID #{chamado.Id}.",
                Prioridade = "Media",
                ChamadoId = chamado.Id,
                Acao = "visualizar_chamado"
            };

            await CriarNotificacaoAsync(notificacao);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao notificar criação do chamado {ChamadoId}: ex");
        }
    }

    public async Task NotificarChamadoAtribuidoAsync(Chamado chamado, Usuario tecnico)
    {
        try
        {
            // Notificar o usuário que criou o chamado
            var notificacaoUsuario = new Notificacao
            {
                UsuarioId = chamado.UsuarioId,
                Tipo = "chamado_atribuido",
                Titulo = "Chamado Atribuído",
                Mensagem = $"Seu chamado '{chamado.Titulo}' foi atribuído ao técnico {tecnico.Nome}.",
                Prioridade = "Media",
                ChamadoId = chamado.Id,
                Acao = "visualizar_chamado"
            };

            await CriarNotificacaoAsync(notificacaoUsuario);

            // Notificar o técnico
            var notificacaoTecnico = new Notificacao
            {
                UsuarioId = tecnico.Id,
                Tipo = "novo_chamado_atribuido",
                Titulo = "Novo Chamado Atribuído",
                Mensagem = $"Você recebeu um novo chamado: '{chamado.Titulo}' (ID #{chamado.Id}).",
                Prioridade = chamado.IsUrgente ? "Alta" : "Media",
                ChamadoId = chamado.Id,
                Acao = "atender_chamado"
            };

            await CriarNotificacaoAsync(notificacaoTecnico);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao notificar atribuição do chamado {ChamadoId}: ex");
        }
    }

    public async Task NotificarChamadoResolvidoAsync(Chamado chamado)
    {
        try
        {
            var notificacao = new Notificacao
            {
                UsuarioId = chamado.UsuarioId,
                Tipo = "chamado_resolvido",
                Titulo = "Chamado Resolvido",
                Mensagem = $"Seu chamado '{chamado.Titulo}' foi resolvido. Por favor, confirme se a solução atende suas necessidades.",
                Prioridade = "Media",
                ChamadoId = chamado.Id,
                Acao = "avaliar_chamado"
            };

            await CriarNotificacaoAsync(notificacao);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao notificar resolução do chamado {ChamadoId}: ex");
        }
    }

    public async Task NotificarChamadoComentadoAsync(Chamado chamado, Comentario comentario)
    {
        try
        {
            // Notificar apenas se o comentário não foi feito pelo próprio usuário do chamado
            if (comentario.UsuarioId != chamado.UsuarioId)
            {
                var usuarioComentario = await _context.Usuarios.FindAsync(comentario.UsuarioId);
                var notificacao = new Notificacao
                {
                    UsuarioId = chamado.UsuarioId,
                    Tipo = "chamado_comentado",
                    Titulo = "Novo Comentário",
                    Mensagem = $"Há um novo comentário no seu chamado '{chamado.Titulo}' por {usuarioComentario?.Nome}.",
                    Prioridade = "Media",
                    ChamadoId = chamado.Id,
                    Acao = "visualizar_comentarios"
                };

                await CriarNotificacaoAsync(notificacao);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao notificar comentário no chamado {ChamadoId}: ex");
        }
    }

    public async Task NotificarStatusAlteradoAsync(Chamado chamado, string statusAnterior)
    {
        try
        {
            var notificacao = new Notificacao
            {
                UsuarioId = chamado.UsuarioId,
                Tipo = "status_alterado",
                Titulo = "Status Alterado",
                Mensagem = $"O status do seu chamado '{chamado.Titulo}' foi alterado de '{statusAnterior}' para '{chamado.Status}'.",
                Prioridade = "Media",
                ChamadoId = chamado.Id,
                Acao = "visualizar_chamado"
            };

            await CriarNotificacaoAsync(notificacao);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao notificar alteração de status do chamado {ChamadoId}: ex");
        }
    }

    #endregion

    #region Configurações de Notificação

    public async Task<ConfiguracaoNotificacao> ObterConfiguracaoUsuarioAsync(int usuarioId)
    {
        var configuracao = await _context.ConfiguracoesNotificacao
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (configuracao == null)
        {
            configuracao = await CriarConfiguracaoPadraoAsync(usuarioId);
        }

        return configuracao;
    }

    public async Task<ConfiguracaoNotificacao> AtualizarConfiguracaoAsync(ConfiguracaoNotificacao configuracao)
    {
        try
        {
            configuracao.DataAtualizacao = DateTime.Now;
            _context.ConfiguracoesNotificacao.Update(configuracao);
            await _context.SaveChangesAsync();
            return configuracao;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao atualizar configuração de notificação para usuário {UsuarioId}: ex");
            throw;
        }
    }

    public async Task<ConfiguracaoNotificacao> CriarConfiguracaoPadraoAsync(int usuarioId)
    {
        try
        {
            var configuracao = new ConfiguracaoNotificacao
            {
                UsuarioId = usuarioId,
                EmailAtivo = true,
                PushAtivo = true,
                InAppAtivo = true,
                NotificarStatus = true,
                NotificarAtribuicao = true,
                NotificarResolucao = true,
                NotificarComentarios = true,
                NotificarEscalacao = true,
                Frequencia = "Imediato",
                DataCriacao = DateTime.Now
            };

            _context.ConfiguracoesNotificacao.Add(configuracao);
            await _context.SaveChangesAsync();

            return configuracao;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao criar configuração padrão para usuário {UsuarioId}: ex");
            throw;
        }
    }

    #endregion

    #region Envio de Notificações

    public async Task<bool> EnviarNotificacaoEmailAsync(Notificacao notificacao)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(notificacao.UsuarioId);
            if (usuario == null) return false;

            var configuracao = await ObterConfiguracaoUsuarioAsync(notificacao.UsuarioId);
            if (!configuracao.EmailAtivo) return false;

            var request = new NotificacaoRequest
            {
                Tipo = notificacao.Tipo,
                Destinatario = new Destinatario
                {
                    Email = usuario.Email,
                    Nome = usuario.Nome
                }
            };

            if (notificacao.ChamadoId.HasValue)
            {
                var chamado = await _context.Chamados.FindAsync(notificacao.ChamadoId);
                if (chamado != null)
                {
                    request.Chamado = new ChamadoInfo
                    {
                        Id = chamado.Id,
                        Titulo = chamado.Titulo,
                        Status = chamado.Status,
                        Prioridade = chamado.Prioridade
                    };
                }
            }

            var response = await _javaApiClient.EnviarNotificacaoAsync(request);
            return response.Status == "enviada";
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao enviar notificação por email para usuário {UsuarioId}: ex");
            return false;
        }
    }

    public async Task<bool> EnviarNotificacaoPushAsync(Notificacao notificacao)
    {
        try
        {
            // Implementar envio de push notification
            // Por enquanto, apenas log
            Console.WriteLine("Enviando push notification para usuário {UsuarioId}: {Titulo}", 
                notificacao.UsuarioId, notificacao.Titulo);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao enviar notificação push para usuário {UsuarioId}: ex");
            return false;
        }
    }

    public async Task ProcessarFilaNotificacoesAsync()
    {
        try
        {
            var notificacoesPendentes = await _context.Notificacoes
                .Where(n => !n.Lida && n.DataEnvio <= DateTime.Now.AddMinutes(-5))
                .Take(100)
                .ToListAsync();

            foreach (var notificacao in notificacoesPendentes)
            {
                await EnviarPorCanaisAsync(notificacao);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao processar fila de notificações: ex");
        }
    }

    #endregion

    #region Relatórios

    public async Task<List<Notificacao>> ObterEstatisticasAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.Notificacoes
            .Where(n => n.DataEnvio >= dataInicio && n.DataEnvio <= dataFim)
            .Include(n => n.Usuario)
            .OrderByDescending(n => n.DataEnvio)
            .ToListAsync();
    }

    #endregion

    #region Métodos Auxiliares

    private async Task EnviarPorCanaisAsync(Notificacao notificacao)
    {
        var configuracao = await ObterConfiguracaoUsuarioAsync(notificacao.UsuarioId);

        // Verificar se deve enviar baseado na configuração
        if (!DeveEnviarNotificacao(notificacao, configuracao))
            return;

        // Verificar horário silencioso
        if (configuracao.IsSilenciosoAgora && notificacao.Prioridade != "Urgente")
            return;

        // Enviar por email se configurado
        if (configuracao.EmailAtivo)
        {
            await EnviarNotificacaoEmailAsync(notificacao);
        }

        // Enviar push se configurado
        if (configuracao.PushAtivo)
        {
            await EnviarNotificacaoPushAsync(notificacao);
        }
    }

    private bool DeveEnviarNotificacao(Notificacao notificacao, ConfiguracaoNotificacao configuracao)
    {
        return notificacao.Tipo switch
        {
            "chamado_criado" => true, // Sempre notificar
            "chamado_atribuido" => configuracao.NotificarAtribuicao,
            "chamado_resolvido" => configuracao.NotificarResolucao,
            "status_alterado" => configuracao.NotificarStatus,
            "chamado_comentado" => configuracao.NotificarComentarios,
            "novo_chamado_atribuido" => configuracao.NotificarAtribuicao,
            _ => true
        };
    }

    #endregion
}
