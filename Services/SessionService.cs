using HelpFastDesktop.Models;

namespace HelpFastDesktop.Services;

public class SessionService : ISessionService
{
    private readonly IUsuarioService _usuarioService;
    private Usuario? _usuarioLogado;

    public SessionService(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public Usuario? UsuarioLogado => _usuarioLogado;
    public bool EstaLogado => _usuarioLogado != null;

    public async Task<bool> FazerLoginAsync(string email, string senha)
    {
        try
        {
            var loginValido = await _usuarioService.ValidarLoginAsync(email, senha);
            if (!loginValido) return false;

            _usuarioLogado = await _usuarioService.ObterPorEmailAsync(email);
            if (_usuarioLogado != null)
            {
                await _usuarioService.AtualizarUltimoLoginAsync(_usuarioLogado.Id);
            }

            return _usuarioLogado != null;
        }
        catch
        {
            return false;
        }
    }

    public void FazerLogout()
    {
        _usuarioLogado = null;
    }

    public async Task<bool> ValidarSessaoAsync()
    {
        if (_usuarioLogado == null) return false;

        try
        {
            var usuario = await _usuarioService.ObterPorIdAsync(_usuarioLogado.Id);
            if (usuario == null || !usuario.Ativo)
            {
                FazerLogout();
                return false;
            }

            _usuarioLogado = usuario; // Atualizar dados do usuário
            return true;
        }
        catch
        {
            FazerLogout();
            return false;
        }
    }
}

