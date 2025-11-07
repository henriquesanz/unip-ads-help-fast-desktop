using System.Text;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using HelpFastDesktop.Core.Models.Entities.JavaApi;
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

    public async Task<string> GetN8nWebhookUrlAsync()
    {
        var config = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave == "n8n_webhook_chat_url");
        // URL do webhook do n8n para chat com IA
        return config?.Valor ?? "https://n8n.grupoopt.com.br/webhook-test/58ab6b56-d816-48eb-a2cc-b0601d1e6d11";
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
            // Usar webhook do n8n para chat
            var webhookUrl = await GetN8nWebhookUrlAsync();

            // Preparar requisição no formato esperado pelo webhook n8n
            // O n8n geralmente espera os dados no body da requisição POST
            var request = new
            {
                usuarioId = usuarioId,
                mensagem = mensagem,
                contexto = contexto,
                // Adicionar timestamp para rastreamento
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            Console.WriteLine($"[CHAT IA] Enviando mensagem para webhook n8n: {webhookUrl}");
            Console.WriteLine($"[CHAT IA] UsuarioId: {usuarioId}, Mensagem: {mensagem}");

            // Chamar webhook do n8n e processar resposta
            var response = await MakeRequestWithFlexibleParsingAsync(webhookUrl, HttpMethod.Post, request);
            
            if (response != null && !string.IsNullOrEmpty(response.Resposta))
            {
                Console.WriteLine($"[CHAT IA] Resposta recebida com sucesso: {response.Resposta.Substring(0, Math.Min(100, response.Resposta.Length))}...");
                return response;
            }

            // Se a resposta não vier no formato esperado, retornar resposta padrão
            Console.WriteLine("[CHAT IA] AVISO: Resposta do webhook n8n não pôde ser processada corretamente");
            return new ChatResponse 
            { 
                Resposta = "Desculpe, não consegui processar sua mensagem. Tente novamente mais tarde.",
                EscalarParaHumano = true
            };
        }
        catch (HttpRequestException httpEx)
        {
            var errorMessage = $"Erro HTTP ao comunicar com webhook n8n: {httpEx.Message}";
            if (httpEx.InnerException != null)
            {
                errorMessage += $" | Erro interno: {httpEx.InnerException.Message}";
            }
            Console.WriteLine($"[CHAT IA] ERRO HTTP: {errorMessage}");
            Console.WriteLine($"[CHAT IA] Stack trace: {httpEx.StackTrace}");
            
            return new ChatResponse 
            { 
                Resposta = $"Erro de comunicação com o serviço de IA. Detalhes: {httpEx.Message}. Por favor, verifique sua conexão e tente novamente.",
                EscalarParaHumano = true
            };
        }
        catch (TaskCanceledException timeoutEx)
        {
            var errorMessage = $"Timeout ao comunicar com webhook n8n: {timeoutEx.Message}";
            Console.WriteLine($"[CHAT IA] ERRO TIMEOUT: {errorMessage}");
            Console.WriteLine($"[CHAT IA] Stack trace: {timeoutEx.StackTrace}");
            
            return new ChatResponse 
            { 
                Resposta = "O serviço de IA demorou muito para responder. Por favor, tente novamente mais tarde.",
                EscalarParaHumano = true
            };
        }
        catch (Exception ex)
        {
            var errorMessage = $"Erro inesperado ao enviar mensagem de chat para usuário {usuarioId}: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $" | Erro interno: {ex.InnerException.Message}";
            }
            Console.WriteLine($"[CHAT IA] ERRO GERAL: {errorMessage}");
            Console.WriteLine($"[CHAT IA] Tipo de exceção: {ex.GetType().Name}");
            Console.WriteLine($"[CHAT IA] Stack trace: {ex.StackTrace}");
            
            return new ChatResponse 
            { 
                Resposta = $"Erro ao processar sua mensagem. Detalhes técnicos: {ex.GetType().Name} - {ex.Message}. Por favor, tente novamente ou entre em contato com o suporte.",
                EscalarParaHumano = true
            };
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

    private async Task<ChatResponse?> MakeRequestWithFlexibleParsingAsync(string url, HttpMethod method, object? content = null)
    {
        var retryAttempts = await GetRetryAttemptsAsync();
        var timeout = await GetTimeoutAsync();

        for (int attempt = 1; attempt <= retryAttempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(method, url);

                // Adicionar headers padrão
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("User-Agent", "HelpFastDesktop/1.0");

                if (content != null)
                {
                    var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    
                    Console.WriteLine($"[HTTP REQUEST] Tentativa {attempt}/{retryAttempts}");
                    Console.WriteLine($"[HTTP REQUEST] URL: {url}");
                    Console.WriteLine($"[HTTP REQUEST] Método: {method.Method}");
                    Console.WriteLine($"[HTTP REQUEST] Body JSON: {json}");
                    
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                else
                {
                    Console.WriteLine($"[HTTP REQUEST] Tentativa {attempt}/{retryAttempts}");
                    Console.WriteLine($"[HTTP REQUEST] URL: {url}");
                    Console.WriteLine($"[HTTP REQUEST] Método: {method.Method}");
                    Console.WriteLine($"[HTTP REQUEST] Body: (vazio)");
                }

                // Configurar timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));

                var response = await _httpClient.SendAsync(request, cts.Token);

                Console.WriteLine($"[HTTP RESPONSE] Status Code: {(int)response.StatusCode} ({response.StatusCode})");
                Console.WriteLine($"[HTTP RESPONSE] Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(";", h.Value)}"))}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resposta bruta do webhook n8n: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}...");
                    
                    // Tentar deserializar como ChatResponse padrão
                    try
                    {
                        var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        
                        if (chatResponse != null && !string.IsNullOrEmpty(chatResponse.Resposta))
                        {
                            Console.WriteLine("Resposta parseada com sucesso usando formato ChatResponse padrão");
                            return chatResponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Tentativa de deserialização padrão falhou: {ex.Message}");
                    }

                    // Tentar parsear como resposta do n8n (pode vir em diferentes formatos)
                    try
                    {
                        using var doc = JsonDocument.Parse(responseContent);
                        var root = doc.RootElement;

                        var chatResponse = new ChatResponse();

                        // Se a resposta for um array, pegar o primeiro elemento
                        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                        {
                            root = root[0];
                        }

                        // Tentar diferentes formatos de resposta do n8n
                        if (root.TryGetProperty("resposta", out var respostaProp))
                        {
                            chatResponse.Resposta = ExtractStringValue(respostaProp);
                        }
                        else if (root.TryGetProperty("message", out var messageProp))
                        {
                            chatResponse.Resposta = ExtractStringValue(messageProp);
                        }
                        else if (root.TryGetProperty("text", out var textProp))
                        {
                            chatResponse.Resposta = ExtractStringValue(textProp);
                        }
                        else if (root.TryGetProperty("output", out var outputProp))
                        {
                            // n8n pode retornar dados em "output"
                            chatResponse.Resposta = ExtractStringValue(outputProp);
                        }
                        else if (root.TryGetProperty("data", out var dataProp))
                        {
                            // n8n pode retornar dados em "data"
                            if (dataProp.TryGetProperty("resposta", out var dataResposta))
                            {
                                chatResponse.Resposta = ExtractStringValue(dataResposta);
                            }
                            else if (dataProp.TryGetProperty("message", out var dataMessage))
                            {
                                chatResponse.Resposta = ExtractStringValue(dataMessage);
                            }
                            else
                            {
                                chatResponse.Resposta = ExtractStringValue(dataProp);
                            }
                        }
                        else if (root.ValueKind == JsonValueKind.String)
                        {
                            // Se a resposta for uma string simples
                            chatResponse.Resposta = root.GetString() ?? string.Empty;
                        }
                        else
                        {
                            // Tentar usar o primeiro valor encontrado ou o JSON completo
                            chatResponse.Resposta = responseContent.Length > 1000 
                                ? responseContent.Substring(0, 1000) + "..." 
                                : responseContent;
                        }

                        // Verificar se há indicação de escalação
                        if (root.TryGetProperty("escalarParaHumano", out var escalarProp))
                        {
                            chatResponse.EscalarParaHumano = escalarProp.GetBoolean();
                        }
                        else if (root.TryGetProperty("escalate", out var escalateProp))
                        {
                            chatResponse.EscalarParaHumano = escalateProp.GetBoolean();
                        }
                        else if (root.TryGetProperty("escalar", out var escalarProp2))
                        {
                            chatResponse.EscalarParaHumano = escalarProp2.GetBoolean();
                        }

                        // Verificar categoria
                        if (root.TryGetProperty("categoria", out var categoriaProp))
                        {
                            chatResponse.Categoria = ExtractStringValue(categoriaProp);
                        }
                        else if (root.TryGetProperty("category", out var categoryProp))
                        {
                            chatResponse.Categoria = ExtractStringValue(categoryProp);
                        }

                        // Verificar confiança
                        if (root.TryGetProperty("confianca", out var confiancaProp))
                        {
                            chatResponse.Confianca = ExtractDecimalValue(confiancaProp);
                        }
                        else if (root.TryGetProperty("confidence", out var confidenceProp))
                        {
                            chatResponse.Confianca = ExtractDecimalValue(confidenceProp);
                        }

                        // Verificar sugestões FAQ
                        if (root.TryGetProperty("sugestoesFAQ", out var sugestoesProp) && sugestoesProp.ValueKind == JsonValueKind.Array)
                        {
                            chatResponse.SugestoesFAQ = sugestoesProp.EnumerateArray()
                                .Select(e => ExtractStringValue(e))
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList()!;
                        }
                        else if (root.TryGetProperty("faqSuggestions", out var faqProp) && faqProp.ValueKind == JsonValueKind.Array)
                        {
                            chatResponse.SugestoesFAQ = faqProp.EnumerateArray()
                                .Select(e => ExtractStringValue(e))
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList()!;
                        }

                        if (!string.IsNullOrEmpty(chatResponse.Resposta))
                        {
                            Console.WriteLine("Resposta parseada com sucesso usando parsing flexível");
                            return chatResponse;
                        }
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"Erro ao parsear resposta do webhook n8n: {parseEx.Message}");
                        Console.WriteLine($"Stack trace: {parseEx.StackTrace}");
                    }

                    // Se não conseguiu parsear, retornar resposta genérica com o conteúdo recebido
                    Console.WriteLine("Não foi possível parsear a resposta, retornando conteúdo bruto");
                    return new ChatResponse
                    {
                        Resposta = responseContent.Length > 500 ? responseContent.Substring(0, 500) + "..." : responseContent,
                        EscalarParaHumano = false
                    };
                }

                // Ler conteúdo da resposta de erro para diagnóstico
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[HTTP ERROR] Tentativa {attempt}/{retryAttempts} falhou");
                Console.WriteLine($"[HTTP ERROR] Status Code: {(int)response.StatusCode} ({response.StatusCode})");
                Console.WriteLine($"[HTTP ERROR] URL: {url}");
                Console.WriteLine($"[HTTP ERROR] Resposta do servidor: {errorContent.Substring(0, Math.Min(500, errorContent.Length))}");

                if (attempt == retryAttempts)
                {
                    Console.WriteLine($"[HTTP ERROR] Todas as {retryAttempts} tentativas falharam para URL {url}");
                    Console.WriteLine($"[HTTP ERROR] Última resposta: Status {response.StatusCode}, Conteúdo: {errorContent}");
                    
                    // Retornar resposta de erro mais detalhada
                    return new ChatResponse
                    {
                        Resposta = $"Erro ao comunicar com o serviço de IA. Status HTTP: {(int)response.StatusCode} ({response.StatusCode}). " +
                                  $"Por favor, verifique sua conexão ou tente novamente mais tarde.",
                        EscalarParaHumano = true
                    };
                }

                // Aguardar antes da próxima tentativa (exponential backoff)
                var delaySeconds = Math.Pow(2, attempt - 1);
                Console.WriteLine($"[HTTP RETRY] Aguardando {delaySeconds} segundos antes da próxima tentativa...");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
            catch (HttpRequestException httpEx)
            {
                var errorMessage = $"Erro HTTP na tentativa {attempt}/{retryAttempts}: {httpEx.Message}";
                if (httpEx.InnerException != null)
                {
                    errorMessage += $" | Erro interno: {httpEx.InnerException.Message}";
                }
                Console.WriteLine($"[HTTP EXCEPTION] {errorMessage}");
                Console.WriteLine($"[HTTP EXCEPTION] URL: {url}");
                Console.WriteLine($"[HTTP EXCEPTION] Stack trace: {httpEx.StackTrace}");

                if (attempt == retryAttempts)
                {
                    throw new HttpRequestException(
                        $"Falha ao comunicar com webhook n8n após {retryAttempts} tentativas. " +
                        $"URL: {url}. Erro: {httpEx.Message}", 
                        httpEx);
                }
            }
            catch (TaskCanceledException timeoutEx)
            {
                Console.WriteLine($"[HTTP TIMEOUT] Timeout na tentativa {attempt}/{retryAttempts} para URL {url}");
                Console.WriteLine($"[HTTP TIMEOUT] Timeout configurado: {timeout} segundos");
                Console.WriteLine($"[HTTP TIMEOUT] Erro: {timeoutEx.Message}");

                if (attempt == retryAttempts)
                {
                    throw new TimeoutException(
                        $"Timeout ao comunicar com webhook n8n após {retryAttempts} tentativas. " +
                        $"URL: {url}. Timeout configurado: {timeout} segundos.", 
                        timeoutEx);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Erro inesperado na tentativa {attempt}/{retryAttempts} para URL {url}: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Erro interno: {ex.InnerException.Message}";
                }
                Console.WriteLine($"[HTTP EXCEPTION] {errorMessage}");
                Console.WriteLine($"[HTTP EXCEPTION] Tipo: {ex.GetType().Name}");
                Console.WriteLine($"[HTTP EXCEPTION] Stack trace: {ex.StackTrace}");

                if (attempt == retryAttempts)
                    throw;
            }
        }

        Console.WriteLine($"[HTTP ERROR] Todas as tentativas foram esgotadas sem sucesso para URL {url}");
        return null;
    }

    private static string ExtractStringValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Object => element.GetRawText(),
            JsonValueKind.Array => string.Join(", ", element.EnumerateArray().Select(ExtractStringValue)),
            _ => element.GetRawText()
        };
    }

    private static decimal? ExtractDecimalValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.GetDecimal();
        }
        else if (element.ValueKind == JsonValueKind.String && decimal.TryParse(element.GetString(), out var decimalValue))
        {
            return decimalValue;
        }
        return null;
    }

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
