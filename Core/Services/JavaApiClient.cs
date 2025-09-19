using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Entities;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Entities.JavaApi;
using System.Net.Http;

namespace HelpFastDesktop.Core.Services;

public class JavaApiClient : IJavaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;

    public JavaApiClient(HttpClient httpClient, ApplicationDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    #region Configurações

    public async Task<string> GetBaseUrlAsync()
    {
        var config = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave == "api_java_base_url");
        return config?.Valor ?? "https://helpfast-java-api.oraclecloud.com/api";
    }

    public async Task<int> GetTimeoutAsync()
    {
        var config = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave == "api_java_timeout");
        return int.TryParse(config?.Valor, out var timeout) ? timeout : 30;
    }

    public async Task<int> GetRetryAttemptsAsync()
    {
        var config = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave == "api_java_retry_attempts");
        return int.TryParse(config?.Valor, out var attempts) ? attempts : 3;
    }

    public async Task<bool> IsCategorizacaoAtivaAsync()
    {
        var config = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave == "ia_categorizacao_ativa");
        return bool.TryParse(config?.Valor, out var ativa) && ativa;
    }

    public async Task<bool> IsAtribuicaoAtivaAsync()
    {
        var config = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave == "ia_atribuicao_ativa");
        return bool.TryParse(config?.Valor, out var ativa) && ativa;
    }

    #endregion

    #region Processamento de Chamados

    public async Task<ChamadoProcessamentoResponse?> ProcessarChamadoAsync(Chamado chamado)
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync();
            var url = $"{baseUrl}/chamados/processar";

            var request = new
            {
                id = chamado.Id,
                titulo = chamado.Titulo,
                descricao = chamado.Descricao,
                prioridade = chamado.Prioridade,
                usuarioId = chamado.UsuarioId,
                dataCriacao = chamado.DataCriacao.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            var response = await MakeRequestAsync<ChamadoProcessamentoResponse>(url, HttpMethod.Post, request);
            return response ?? new ChamadoProcessamentoResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao processar chamado {ChamadoId}: ex");
            return new ChamadoProcessamentoResponse();
        }
    }

    public async Task<CategorizacaoResponse?> CategorizarChamadoAsync(Chamado chamado)
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync();
            var url = $"{baseUrl}/categorizacao/analisar";

            var request = new
            {
                chamadoId = chamado.Id,
                titulo = chamado.Titulo,
                descricao = chamado.Descricao,
                prioridade = chamado.Prioridade
            };

            var response = await MakeRequestAsync<CategorizacaoResponse>(url, HttpMethod.Post, request);
            return response ?? new CategorizacaoResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao categorizar chamado {ChamadoId}: ex");
            return new CategorizacaoResponse();
        }
    }

    public async Task<AtribuicaoResponse?> AtribuirChamadoAsync(int chamadoId, List<Usuario> tecnicos)
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync();
            var url = $"{baseUrl}/atribuicao/atribuir";

            var request = new
            {
                chamadoId = chamadoId,
                tecnicos = tecnicos.Select(t => new { id = t.Id, nome = t.Nome, especialidades = new List<string>() })
            };

            var response = await MakeRequestAsync<AtribuicaoResponse>(url, HttpMethod.Post, request);
            return response ?? new AtribuicaoResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao atribuir chamado {ChamadoId}: ex");
            return new AtribuicaoResponse();
        }
    }

    #endregion

    #region Chat com IA

    public async Task<ChatResponse?> EnviarMensagemChatAsync(int usuarioId, string mensagem, Dictionary<string, object> contexto)
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync();
            var url = $"{baseUrl}/chat/mensagem";

            var request = new
            {
                usuarioId = usuarioId,
                mensagem = mensagem,
                contexto = contexto
            };

            var response = await MakeRequestAsync<ChatResponse>(url, HttpMethod.Post, request);
            return response ?? new ChatResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao enviar mensagem de chat para usuário {UsuarioId}: ex");
            return new ChatResponse { Resposta = "Desculpe, ocorreu um erro ao processar sua mensagem." };
        }
    }

    public async Task<bool> ValidarCategorizacaoAsync(int chamadoId, string categoriaOriginal, string categoriaCorrigida)
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync();
            var url = $"{baseUrl}/categorizacao/validar";

            var request = new
            {
                chamadoId = chamadoId,
                categoriaOriginal = categoriaOriginal,
                categoriaCorrigida = categoriaCorrigida
            };

            var response = await MakeRequestAsync<object>(url, HttpMethod.Post, request);
            return response != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao validar categorização do chamado {ChamadoId}: ex");
            return false;
        }
    }

    #endregion

    #region Análise de Padrões

    public async Task<AnalisePadroesResponse?> AnalisarPadroesAsync(DateTime dataInicio, DateTime dataFim, string? categoria = null)
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync();
            var url = $"{baseUrl}/analise/padroes";

            var queryParams = new List<string>
            {
                $"dataInicio={dataInicio:yyyy-MM-ddTHH:mm:ssZ}",
                $"dataFim={dataFim:yyyy-MM-ddTHH:mm:ssZ}"
            };

            if (!string.IsNullOrEmpty(categoria))
                queryParams.Add($"categoria={Uri.EscapeDataString(categoria)}");

            url += "?" + string.Join("&", queryParams);

            var response = await MakeRequestAsync<AnalisePadroesResponse>(url, HttpMethod.Get);
            return response ?? new AnalisePadroesResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao analisar padrões de {DataInicio} a {DataFim}: ex");
            return new AnalisePadroesResponse();
        }
    }

    #endregion

    #region Notificações

    public async Task<NotificacaoResponse?> EnviarNotificacaoAsync(NotificacaoRequest request)
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync();
            var url = $"{baseUrl}/notificacoes/enviar";

            var response = await MakeRequestAsync<NotificacaoResponse>(url, HttpMethod.Post, request);
            return response ?? new NotificacaoResponse();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao enviar notificação para {Email}: ex");
            return new NotificacaoResponse();
        }
    }

    #endregion

    #region Health Check

    public async Task<HealthCheckResponse?> VerificarSaudeAsync()
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync();
            var url = $"{baseUrl}/health";

            var response = await MakeRequestAsync<HealthCheckResponse>(url, HttpMethod.Get);
            return response ?? new HealthCheckResponse { Status = "unhealthy" };
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao verificar saúde da API: ex");
            return new HealthCheckResponse { Status = "unhealthy" };
        }
    }

    #endregion

    #region Métodos Auxiliares

    private async Task<T?> MakeRequestAsync<T>(string url, HttpMethod method, object? content = null) where T : class
    {
        var retryAttempts = await GetRetryAttemptsAsync();
        var timeout = await GetTimeoutAsync();

        for (int attempt = 1; attempt <= retryAttempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);

                if (content != null)
                {
                    var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                // Configurar timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));

                var response = await _httpClient.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }

                Console.WriteLine("Tentativa {Attempt}/{MaxAttempts} falhou com status {StatusCode} para URL {Url}",
                    attempt, retryAttempts, response.StatusCode, url);

                if (attempt == retryAttempts)
                {
                    Console.WriteLine("Todas as tentativas falharam para URL {Url}", url);
                    return null;
                }

                // Aguardar antes da próxima tentativa (exponential backoff)
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)));
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Timeout na tentativa {Attempt}/{MaxAttempts} para URL {Url}",
                    attempt, retryAttempts, url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na tentativa {attempt}/{retryAttempts} para URL {url}: {ex.Message}");

                if (attempt == retryAttempts)
                    throw;
            }
        }

        return null;
    }

    #endregion
}
