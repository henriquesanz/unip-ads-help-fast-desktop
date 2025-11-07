using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;

namespace HelpFastDesktop.Core.Services;

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
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            System.Diagnostics.Debug.WriteLine("Email ou senha vazios no login");
            return false;
        }

        try
        {
            // Obter usuário do banco (fazendo consulta única na tabela Usuarios)
            var usuario = await _usuarioService.ObterPorEmailAsync(email);
            if (usuario == null)
            {
                System.Diagnostics.Debug.WriteLine($"Usuário não encontrado para email: {email}");
                return false;
            }

            // Validar senha
            if (usuario.Senha != senha)
            {
                System.Diagnostics.Debug.WriteLine($"Senha inválida para email: {email}");
                return false;
            }

            // Login válido - definir usuário logado
            _usuarioLogado = usuario;

            // Atualizar último login
            await _usuarioService.AtualizarUltimoLoginAsync(_usuarioLogado.Id);

            System.Diagnostics.Debug.WriteLine($"Login bem-sucedido para usuário: {_usuarioLogado.Nome} (ID: {_usuarioLogado.Id})");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao fazer login: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
            if (usuario == null) // Ativo não existe no banco
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

