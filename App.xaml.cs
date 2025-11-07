using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Services;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Presentation.Controllers;
using HelpFastDesktop.Presentation.Views;
using Microsoft.EntityFrameworkCore;
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

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_ERROR_HANDLE = -12;
    private const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Tentar anexar ao console do processo pai primeiro (quando executado via dotnet run)
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                // Se não conseguir anexar, criar um novo console
                AllocConsole();
            }

            // Garantir que stdout e stderr estão configurados
            var stdoutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            var stderrHandle = GetStdHandle(STD_ERROR_HANDLE);
            
            // Redirecionar Console.Out e Console.Error para garantir que funcionem
            try
            {
                Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                Console.SetError(new System.IO.StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
            }
            catch
            {
                // Se falhar, continuar sem redirecionar
            }

            // Criar arquivo de log como backup
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"app_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            var logFile = new System.IO.StreamWriter(logPath, append: true) { AutoFlush = true };
            
            // Criar um TextWriter que escreve tanto no console quanto no arquivo
            var dualWriter = new DualTextWriter(Console.Out, logFile);
            Console.SetOut(dualWriter);
            Console.SetError(dualWriter);

            Console.WriteLine("=== HelpFast Desktop - Logs de Debug ===");
            Console.WriteLine($"Iniciando aplicação em {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Console alocado: {stdoutHandle != IntPtr.Zero}");
            Console.WriteLine($"Logs também salvos em: {logPath}");
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
                // String de conexão para Azure SQL
                var connectionString = "Server=help-fast-server.database.windows.net,1433;" +
                                      "Database=help-fast-database;" +
                                      "User Id=help-fast-admin;" +
                                      "Password=23568974123@Pim;" +
                                      "Encrypt=True;" +
                                      "TrustServerCertificate=False;" +
                                      "Connection Timeout=30;" +
                                      "Command Timeout=30;";

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(30);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });
                    // Não criar banco automaticamente - as tabelas já existem no Azure SQL
                    options.EnableSensitiveDataLogging(false);
                });

                // Registrar HttpClient para JavaApiClient
                services.AddHttpClient<IJavaApiClient, JavaApiClient>();

                // Registrar serviços
                services.AddScoped<ISessionService, SessionService>();
                services.AddScoped<IUsuarioService, UsuarioService>();
                services.AddScoped<IChamadoService, ChamadoService>();
                services.AddScoped<INotificacaoService, NotificacaoService>();
                services.AddScoped<IAIService, AIService>();
                services.AddScoped<IFAQService, FAQService>();
                services.AddScoped<IRelatorioService, RelatorioService>();
                services.AddScoped<IAuditoriaService, AuditoriaService>();
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
