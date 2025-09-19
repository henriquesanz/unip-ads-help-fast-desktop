namespace HelpFastDesktop.Core.Entities;

public enum UserRole
{
    Cliente = 1,
    Tecnico = 2,
    Administrador = 3
}

public static class UserRoleExtensions
{
    public static string GetDisplayName(this UserRole role)
    {
        return role switch
        {
            UserRole.Cliente => "Cliente",
            UserRole.Tecnico => "Técnico",
            UserRole.Administrador => "Administrador",
            _ => "Desconhecido"
        };
    }

    public static string GetDescription(this UserRole role)
    {
        return role switch
        {
            UserRole.Cliente => "Pode abrir e visualizar seus próprios chamados",
            UserRole.Tecnico => "Pode visualizar e resolver todos os chamados",
            UserRole.Administrador => "Pode gerenciar usuários e configurações do sistema",
            _ => "Papel não definido"
        };
    }

    public static bool CanRegisterSelf(this UserRole role)
    {
        return role == UserRole.Cliente;
    }

    public static bool CanManageUsers(this UserRole role)
    {
        return role == UserRole.Administrador;
    }

    public static bool CanViewAllTickets(this UserRole role)
    {
        return role == UserRole.Tecnico || role == UserRole.Administrador;
    }
}

