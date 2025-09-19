using HelpFastDesktop.Data;
using HelpFastDesktop.Services;
using HelpFastDesktop.ViewModels;
using HelpFastDesktop.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace HelpFastDesktop;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        // Inicializar o banco de dados em memória
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
        }

        // Criar e mostrar a tela de login
        var loginViewModel = new LoginViewModel(ServiceProvider.GetRequiredService<ISessionService>());
        var loginView = new LoginView(loginViewModel);
        loginView.Show();

        base.OnStartup(e);
    }

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("HelpFastDesktopDB");
                });

                // Registrar serviços
                services.AddScoped<ISessionService, SessionService>();
                services.AddScoped<IUsuarioService, UsuarioService>();
                services.AddScoped<IChamadoService, ChamadoService>();
                services.AddScoped<INotificacaoService, NotificacaoService>();
                services.AddScoped<IAIService, AIService>();
                services.AddScoped<IFAQService, FAQService>();
                services.AddScoped<IRelatorioService, RelatorioService>();
                services.AddScoped<IAuditoriaService, AuditoriaService>();
                services.AddScoped<IJavaApiClient, JavaApiClient>();
            });
    }
}
