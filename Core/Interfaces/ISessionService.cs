using HelpFastDesktop.Core.Entities;

namespace HelpFastDesktop.Core.Interfaces;

public interface ISessionService
{
    Usuario? UsuarioLogado { get; }
    bool EstaLogado { get; }
    
    Task<bool> FazerLoginAsync(string email, string senha);
    void FazerLogout();
    Task<bool> ValidarSessaoAsync();
}

