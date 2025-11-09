using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Models;
using HelpFastDesktop.Core.Services;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Presentation.Controllers;
using HelpFastDesktop.Presentation.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace HelpFastDesktop;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    // Importar função do Windows para alocar console
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AttachConsole(uint dwProcessId);

    private const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDirectory);
            var logPath = Path.Combine(logsDirectory, $"app_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            var logFileWriter = new StreamWriter(logPath, append: true) { AutoFlush = true };

#if DEBUG
            var consoleAttached = AttachConsole(ATTACH_PARENT_PROCESS);
            if (!consoleAttached)
            {
                consoleAttached = AllocConsole();
            }

            if (consoleAttached)
            {
                try
                {
                    var consoleWriter = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                    var dualWriter = new DualTextWriter(consoleWriter, logFileWriter);
                    Console.SetOut(dualWriter);
                    Console.SetError(dualWriter);
                }
                catch
                {
                    Console.SetOut(logFileWriter);
                    Console.SetError(logFileWriter);
                }
            }
            else
            {
                Console.SetOut(logFileWriter);
                Console.SetError(logFileWriter);
            }
#else
            Console.SetOut(logFileWriter);
            Console.SetError(logFileWriter);
#endif

            Console.WriteLine("=== HelpFast Desktop - Logs ===");
            Console.WriteLine($"Iniciando aplicação em {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Logs salvos em: {logPath}");
            Console.WriteLine();
            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            // Criar e mostrar a tela de login primeiro (não bloquear na conexão)
            var loginController = new LoginController(ServiceProvider);
            var loginView = new LoginView(loginController);
            loginView.Show();

            // Verificar conexão com o banco de dados em background (não bloqueia a UI)
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = ServiceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var canConnect = await dbContext.Database.CanConnectAsync();
                    if (!canConnect)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(
                                "Não foi possível conectar ao banco de dados.\n\n" +
                                "Verifique se as credenciais estão corretas e se o servidor está acessível.",
                                "Aviso de Conexão",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            $"Erro ao conectar ao banco de dados:\n{ex.Message}\n\n" +
                            "A aplicação continuará funcionando, mas algumas funcionalidades podem estar limitadas.\n" +
                            "Verifique se as credenciais estão corretas e se o servidor está acessível.\n" +
                            "Certifique-se de que o firewall do Azure SQL permite conexões do seu IP.",
                            "Aviso de Conexão",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    });
                }
            });

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao inicializar a aplicação:\n{ex.Message}\n\n{ex.StackTrace}",
                "Erro Fatal",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                var databaseSection = configuration.GetSection("Database");

                var connectionString = configuration.GetConnectionString("Default")
                    ?? databaseSection.GetValue<string>("ConnectionString")
                    ?? throw new InvalidOperationException(
                        "Configure a string de conexão em ConnectionStrings:Default ou Database:ConnectionString.");

                var commandTimeout = databaseSection.GetValue<int?>("CommandTimeout") ?? 30;
                var enableRetryOnFailure = databaseSection.GetValue<bool?>("EnableRetryOnFailure") ?? true;
                var maxRetryCount = databaseSection.GetValue<int?>("MaxRetryCount") ?? 3;
                var maxRetryDelaySeconds = databaseSection.GetValue<int?>("MaxRetryDelaySeconds") ?? 5;
                var enableSensitiveDataLogging = databaseSection.GetValue<bool?>("EnableSensitiveDataLogging") ?? false;

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(commandTimeout);

                        if (enableRetryOnFailure)
                        {
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: maxRetryCount,
                                maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelaySeconds),
                                errorNumbersToAdd: null);
                        }
                    });
                    // Não criar banco automaticamente - as tabelas já existem no Azure SQL
                    options.EnableSensitiveDataLogging(enableSensitiveDataLogging);
                });

                // Registrar serviços
                services.Configure<GoogleDriveOptions>(context.Configuration.GetSection("GoogleDrive"));
                services.Configure<OpenAIOptions>(context.Configuration.GetSection("OpenAI"));
                services.AddScoped<ISessionService, SessionService>();
                services.AddScoped<IUsuarioService, UsuarioService>();
                services.AddScoped<IChamadoService, ChamadoService>();
                services.AddScoped<INotificacaoService, NotificacaoService>();
                services.AddScoped<IAIService, AIService>();
                services.AddScoped<IFAQService, FAQService>();
                services.AddScoped<IRelatorioService, RelatorioService>();
                services.AddScoped<IAuditoriaService, AuditoriaService>();
                services.AddSingleton<IGoogleDriveService, GoogleDriveService>();
                services.AddHttpClient<IOpenAIService, OpenAIService>();
            });
    }

    // Classe auxiliar para escrever logs tanto no console quanto em arquivo
    private class DualTextWriter : System.IO.TextWriter
    {
        private readonly System.IO.TextWriter _consoleWriter;
        private readonly System.IO.TextWriter _fileWriter;

        public DualTextWriter(System.IO.TextWriter consoleWriter, System.IO.TextWriter fileWriter)
        {
            _consoleWriter = consoleWriter;
            _fileWriter = fileWriter;
        }

        public override System.Text.Encoding Encoding => _consoleWriter.Encoding;

        public override void Write(char value)
        {
            try { _consoleWriter.Write(value); } catch { }
            try { _fileWriter.Write(value); } catch { }
        }

        public override void Write(string? value)
        {
            try { _consoleWriter.Write(value); } catch { }
            try { _fileWriter.Write(value); } catch { }
        }

        public override void WriteLine(string? value)
        {
            try { _consoleWriter.WriteLine(value); } catch { }
            try { _fileWriter.WriteLine(value); } catch { }
        }

        public override void Flush()
        {
            try { _consoleWriter.Flush(); } catch { }
            try { _fileWriter.Flush(); } catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _consoleWriter.Dispose(); } catch { }
                try { _fileWriter.Dispose(); } catch { }
            }
            base.Dispose(disposing);
        }
    }
}
