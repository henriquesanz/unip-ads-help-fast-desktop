using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Entities;
using HelpFastDesktop.Core.Interfaces;

namespace HelpFastDesktop.Core.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly ApplicationDbContext _context;

    public AuditoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Logs de Auditoria

    public async Task LogAcaoAsync(string acao, string tabela, int? registroId, int? usuarioId,
        object? dadosAntigos = null, object? dadosNovos = null, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var log = new LogAuditoria
            {
                UsuarioId = usuarioId,
                Acao = acao,
                Tabela = tabela,
                RegistroId = registroId,
                DadosAntigos = dadosAntigos != null ? JsonSerializer.Serialize(dadosAntigos) : null,
                DadosNovos = dadosNovos != null ? JsonSerializer.Serialize(dadosNovos) : null,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                DataAcao = DateTime.Now
            };

            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar log de auditoria: {acao} em {tabela}: {ex.Message}");
        }
    }

    public async Task<List<LogAuditoria>> ObterLogsAsync(DateTime dataInicio, DateTime dataFim, int? usuarioId = null, string? acao = null)
    {
        var query = _context.LogsAuditoria
            .Include(l => l.Usuario)
            .Where(l => l.DataAcao >= dataInicio && l.DataAcao <= dataFim);

        if (usuarioId.HasValue)
            query = query.Where(l => l.UsuarioId == usuarioId);

        if (!string.IsNullOrEmpty(acao))
            query = query.Where(l => l.Acao == acao);

        return await query
            .OrderByDescending(l => l.DataAcao)
            .ToListAsync();
    }

    public async Task<List<LogAuditoria>> ObterLogsPorTabelaAsync(string tabela, DateTime dataInicio, DateTime dataFim)
    {
        return await _context.LogsAuditoria
            .Include(l => l.Usuario)
            .Where(l => l.Tabela == tabela && l.DataAcao >= dataInicio && l.DataAcao <= dataFim)
            .OrderByDescending(l => l.DataAcao)
            .ToListAsync();
    }

    public async Task<List<LogAuditoria>> ObterLogsPorUsuarioAsync(int usuarioId, DateTime dataInicio, DateTime dataFim)
    {
        return await _context.LogsAuditoria
            .Include(l => l.Usuario)
            .Where(l => l.UsuarioId == usuarioId && l.DataAcao >= dataInicio && l.DataAcao <= dataFim)
            .OrderByDescending(l => l.DataAcao)
            .ToListAsync();
    }

    #endregion

    #region Conformidade LGPD

    public async Task<List<LogAuditoria>> ObterLogsAcessoDadosAsync(int usuarioId, DateTime dataInicio, DateTime dataFim)
    {
        // Logs que indicam acesso a dados pessoais do usuário
        var acoesAcessoDados = new[] { "SELECT", "UPDATE", "DELETE", "VIEW", "EXPORT" };
        
        return await _context.LogsAuditoria
            .Include(l => l.Usuario)
            .Where(l => l.UsuarioId == usuarioId && 
                       l.DataAcao >= dataInicio && 
                       l.DataAcao <= dataFim &&
                       acoesAcessoDados.Contains(l.Acao))
            .OrderByDescending(l => l.DataAcao)
            .ToListAsync();
    }

    public async Task<bool> ExcluirLogsAntigosAsync(DateTime dataLimite)
    {
        try
        {
            var logsAntigos = await _context.LogsAuditoria
                .Where(l => l.DataAcao < dataLimite)
                .ToListAsync();

            _context.LogsAuditoria.RemoveRange(logsAntigos);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Excluídos {logsAntigos.Count} logs de auditoria anteriores a {dataLimite}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao excluir logs antigos anteriores a {dataLimite}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AnonimizarLogsUsuarioAsync(int usuarioId)
    {
        try
        {
            var logsUsuario = await _context.LogsAuditoria
                .Where(l => l.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var log in logsUsuario)
            {
                // Anonimizar dados pessoais mantendo a estrutura do log
                if (!string.IsNullOrEmpty(log.DadosAntigos))
                {
                    log.DadosAntigos = AnonimizarDadosPessoais(log.DadosAntigos);
                }
                if (!string.IsNullOrEmpty(log.DadosNovos))
                {
                    log.DadosNovos = AnonimizarDadosPessoais(log.DadosNovos);
                }
                log.UserAgent = "ANONIMIZADO";
            }

            await _context.SaveChangesAsync();

            Console.WriteLine($"Anonimizados {logsUsuario.Count} logs do usuário {usuarioId}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao anonimizar logs do usuário {usuarioId}: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Relatórios de Auditoria

    public async Task<RelatorioAuditoria> GerarRelatorioAuditoriaAsync(DateTime dataInicio, DateTime dataFim)
    {
        try
        {
            var logs = await _context.LogsAuditoria
                .Include(l => l.Usuario)
                .Where(l => l.DataAcao >= dataInicio && l.DataAcao <= dataFim)
                .ToListAsync();

            var relatorio = new RelatorioAuditoria
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalAcoes = logs.Count,
                UsuariosAtivos = logs.Select(l => l.UsuarioId).Distinct().Count()
            };

            // Ações por tipo
            relatorio.AcoesPorTipo = logs
                .GroupBy(l => l.Acao)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ações por tabela
            relatorio.AcoesPorTabela = logs
                .GroupBy(l => l.Tabela)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ações por usuário
            relatorio.AcoesPorUsuario = logs
                .Where(l => l.Usuario != null)
                .GroupBy(l => l.Usuario!.Nome)
                .ToDictionary(g => g.Key, g => g.Count());

            // Logs críticos (operações de exclusão, alterações de senha, etc.)
            relatorio.LogsCriticos = logs
                .Where(l => l.IsOperacaoCritica)
                .OrderByDescending(l => l.DataAcao)
                .Take(50)
                .ToList();

            // Atividade por usuário
            relatorio.AtividadeUsuarios = logs
                .Where(l => l.Usuario != null)
                .GroupBy(l => l.UsuarioId)
                .Select(g => new AtividadeUsuario
                {
                    UsuarioId = g.Key ?? 0,
                    UsuarioNome = g.First().Usuario!.Nome,
                    TotalAcoes = g.Count(),
                    PrimeiraAcao = g.Min(l => l.DataAcao),
                    UltimaAcao = g.Max(l => l.DataAcao),
                    TiposAcoes = g.Select(l => l.Acao).Distinct().ToList(),
                    TabelasAcessadas = g.Select(l => l.Tabela).Distinct().ToList()
                })
                .OrderByDescending(a => a.TotalAcoes)
                .ToList();

            return relatorio;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao gerar relatório de auditoria de {dataInicio} a {dataFim}: {ex.Message}");
            throw;
        }
    }

    public async Task<List<string>> ObterAcoesRealizadasAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _context.LogsAuditoria
            .Where(l => l.DataAcao >= dataInicio && l.DataAcao <= dataFim)
            .Select(l => l.Acao)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> ObterEstatisticasAcoesAsync(DateTime dataInicio, DateTime dataFim)
    {
        var logs = await _context.LogsAuditoria
            .Where(l => l.DataAcao >= dataInicio && l.DataAcao <= dataFim)
            .ToListAsync();

        return logs
            .GroupBy(l => l.Acao)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    #endregion

    #region Métodos Auxiliares

    private string AnonimizarDadosPessoais(string dadosJson)
    {
        try
        {
            // Campos que contêm dados pessoais e devem ser anonimizados
            var camposPessoais = new[] { "nome", "email", "telefone", "cpf", "rg", "endereco" };
            
            var dados = JsonSerializer.Deserialize<Dictionary<string, object>>(dadosJson);
            if (dados == null) return dadosJson;

            foreach (var campo in camposPessoais)
            {
                if (dados.ContainsKey(campo))
                {
                    dados[campo] = "***ANONIMIZADO***";
                }
            }

            return JsonSerializer.Serialize(dados);
        }
        catch
        {
            // Se não conseguir deserializar, retorna string anonimizada
            return "***DADOS ANONIMIZADOS***";
        }
    }

    #endregion
}
