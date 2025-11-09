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
        Console.WriteLine($"[LOGIN] Iniciando autenticação. Email recebido: '{email ?? "<null>"}'.");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            Console.WriteLine("[LOGIN][ERRO] Email ou senha não informados.");
            return false;
        }

        try
        {
            var credenciaisValidas = await _usuarioService.ValidarLoginAsync(email, senha);
            if (!credenciaisValidas)
            {
                Console.WriteLine($"[LOGIN][ERRO] Credenciais inválidas para o email '{email}'.");
                return false;
            }

            var usuario = await _usuarioService.ObterPorEmailAsync(email);
            if (usuario == null)
            {
                Console.WriteLine($"[LOGIN][ERRO] Usuário não encontrado após validação para o email '{email}'.");
                return false;
            }

            // Login válido - definir usuário logado
            _usuarioLogado = usuario;

            // Atualizar último login
            await _usuarioService.AtualizarUltimoLoginAsync(_usuarioLogado.Id);

            Console.WriteLine($"[LOGIN] Login bem-sucedido para usuário: {_usuarioLogado.Nome} (ID: {_usuarioLogado.Id}).");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGIN][EXCEÇÃO] Erro ao fazer login: {ex.Message}");
            Console.WriteLine($"[LOGIN][EXCEÇÃO] Stack trace: {ex.StackTrace}");
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

