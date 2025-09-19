using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HelpFastDesktop.Core.Entities;

namespace HelpFastDesktop.Presentation.ViewModels;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
            return count > 1 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class UserRoleToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is UserRole role)
        {
            return role switch
            {
                UserRole.Cliente => "Cliente",
                UserRole.Tecnico => "Técnico",
                UserRole.Administrador => "Administrador",
                _ => role.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str switch
            {
                "Cliente" => UserRole.Cliente,
                "Técnico" => UserRole.Tecnico,
                "Administrador" => UserRole.Administrador,
                _ => UserRole.Cliente
            };
        }
        return UserRole.Cliente;
    }
}

public class LoadingToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLoading)
            return isLoading ? "ENTRANDO..." : "ENTRAR";
        return "ENTRAR";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
