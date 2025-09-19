using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Entities;
using HelpFastDesktop.Core.Interfaces;

namespace HelpFastDesktop.Core.Services;

public class RelatorioService : IRelatorioService
{
    private readonly ApplicationDbContext _context;

    public RelatorioService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region CRUD de Relatórios

    public async Task<Relatorio?> ObterPorIdAsync(int id)
    {
        return await _context.Relatorios
            .Include(r => r.CriadoPor)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Relatorio>> ListarTodosAsync()
    {
        return await _context.Relatorios
            .Include(r => r.CriadoPor)
            .OrderByDescending(r => r.DataCriacao)
            .ToListAsync();
    }

    public async Task<List<Relatorio>> ListarPorUsuarioAsync(int usuarioId)
    {
        return await _context.Relatorios
            .Include(r => r.CriadoPor)
            .Where(r => r.CriadoPorId == usuarioId)
            .OrderByDescending(r => r.DataCriacao)
            .ToListAsync();
    }

    public async Task<Relatorio> CriarRelatorioAsync(Relatorio relatorio)
    {
        try
        {
            relatorio.DataCriacao = DateTime.Now;
            _context.Relatorios.Add(relatorio);
            await _context.SaveChangesAsync();
            return relatorio;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao criar relatório: {Nome}: ex");
            throw;
        }
    }

    public async Task<Relatorio> AtualizarRelatorioAsync(Relatorio relatorio)
    {
        try
        {
            _context.Relatorios.Update(relatorio);
            await _context.SaveChangesAsync();
            return relatorio;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao atualizar relatório {RelatorioId}: ex");
            throw;
        }
    }

    public async Task<bool> ExcluirRelatorioAsync(int id)
    {
        try
        {
            var relatorio = await _context.Relatorios.FindAsync(id);
            if (relatorio == null) return false;

            _context.Relatorios.Remove(relatorio);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao excluir relatório {RelatorioId}: ex");
            return false;
        }
    }

    #endregion

    #region Geração de Relatórios

    public async Task<RelatorioPerformance> GerarRelatorioPerformanceAsync(DateTime dataInicio, DateTime dataFim, int? tecnicoId = null)
    {
        try
        {
            var query = _context.Chamados
                .Include(c => c.Usuario)
                .Include(c => c.Tecnico)
                .Where(c => c.DataCriacao >= dataInicio && c.DataCriacao <= dataFim);

            if (tecnicoId.HasValue)
                query = query.Where(c => c.TecnicoId == tecnicoId);

            var chamados = await query.ToListAsync();

            var relatorio = new RelatorioPerformance
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TecnicoId = tecnicoId,
                TecnicoNome = tecnicoId.HasValue ? 
                    (await _context.Usuarios.FindAsync(tecnicoId))?.Nome : null,
                TotalChamados = chamados.Count,
                ChamadosResolvidos = chamados.Count(c => c.Status == "Resolvido" || c.Status == "Fechado"),
                ChamadosEmAndamento = chamados.Count(c => c.Status == "EmAndamento"),
                ChamadosAtrasados = chamados.Count(c => c.IsAtrasado),
                TempoMedioResolucao = chamados.Where(c => c.TempoResolucao.HasValue)
                    .Average(c => (decimal)c.TempoResolucao.Value) / 60, // converter para horas
                TaxaResolucao = chamados.Count > 0 ? 
                    (decimal)chamados.Count(c => c.Status == "Resolvido" || c.Status == "Fechado") / chamados.Count * 100 : 0,
                SatisfacaoMedia = chamados.Where(c => c.Satisfacao.HasValue)
                    .Average(c => (decimal)c.Satisfacao.Value)
            };

            // Performance por prioridade
            relatorio.PerformancePorPrioridade = chamados
                .GroupBy(c => c.Prioridade)
                .Select(g => new PerformancePorPrioridade
                {
                    Prioridade = g.Key,
                    TotalChamados = g.Count(),
                    ChamadosResolvidos = g.Count(c => c.Status == "Resolvido" || c.Status == "Fechado"),
                    TempoMedioResolucao = g.Where(c => c.TempoResolucao.HasValue)
                        .Average(c => (decimal)c.TempoResolucao.Value) / 60,
                    TaxaResolucao = g.Count() > 0 ? 
                        (decimal)g.Count(c => c.Status == "Resolvido" || c.Status == "Fechado") / g.Count() * 100 : 0
                })
                .ToList();

            // Performance por categoria
            relatorio.PerformancePorCategoria = chamados
                .Where(c => !string.IsNullOrEmpty(c.Categoria))
                .GroupBy(c => c.Categoria)
                .Select(g => new PerformancePorCategoria
                {
                    Categoria = g.Key,
                    TotalChamados = g.Count(),
                    ChamadosResolvidos = g.Count(c => c.Status == "Resolvido" || c.Status == "Fechado"),
                    TempoMedioResolucao = g.Where(c => c.TempoResolucao.HasValue)
                        .Average(c => (decimal)c.TempoResolucao.Value) / 60,
                    SatisfacaoMedia = g.Where(c => c.Satisfacao.HasValue)
                        .Average(c => (decimal)c.Satisfacao.Value)
                })
                .ToList();

            // Chamados detalhados
            relatorio.ChamadosDetalhados = chamados
                .Select(c => new ChamadoDetalhado
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    Status = c.Status,
                    Prioridade = c.Prioridade,
                    Categoria = c.Categoria ?? "Não categorizado",
                    DataCriacao = c.DataCriacao,
                    DataResolucao = c.DataResolucao,
                    TempoResolucao = c.TempoResolucao.HasValue ? (decimal)c.TempoResolucao.Value / 60 : 0,
                    Satisfacao = c.Satisfacao,
                    UsuarioNome = c.Usuario?.Nome ?? "N/A",
                    TecnicoNome = c.Tecnico?.Nome
                })
                .ToList();

            return relatorio;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao gerar relatório de performance de {DataInicio} a {DataFim}: ex");
            throw;
        }
    }

    public async Task<RelatorioVolume> GerarRelatorioVolumeAsync(DateTime dataInicio, DateTime dataFim)
    {
        try
        {
            var chamados = await _context.Chamados
                .Include(c => c.Usuario)
                .Include(c => c.Tecnico)
                .Where(c => c.DataCriacao >= dataInicio && c.DataCriacao <= dataFim)
                .ToListAsync();

            var relatorio = new RelatorioVolume
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalChamados = chamados.Count,
                ChamadosAbertos = chamados.Count(c => c.Status == "Aberto"),
                ChamadosResolvidos = chamados.Count(c => c.Status == "Resolvido"),
                ChamadosFechados = chamados.Count(c => c.Status == "Fechado")
            };

            // Volume por dia
            relatorio.VolumePorDia = chamados
                .GroupBy(c => c.DataCriacao.Date)
                .Select(g => new VolumePorDia
                {
                    Data = g.Key,
                    ChamadosAbertos = g.Count(c => c.Status == "Aberto"),
                    ChamadosResolvidos = g.Count(c => c.Status == "Resolvido"),
                    ChamadosFechados = g.Count(c => c.Status == "Fechado")
                })
                .OrderBy(v => v.Data)
                .ToList();

            // Volume por prioridade
            relatorio.VolumePorPrioridade = chamados
                .GroupBy(c => c.Prioridade)
                .Select(g => new VolumePorPrioridade
                {
                    Prioridade = g.Key,
                    Total = g.Count(),
                    Percentual = chamados.Count > 0 ? (decimal)g.Count() / chamados.Count * 100 : 0
                })
                .ToList();

            // Volume por categoria
            relatorio.VolumePorCategoria = chamados
                .Where(c => !string.IsNullOrEmpty(c.Categoria))
                .GroupBy(c => c.Categoria)
                .Select(g => new VolumePorCategoria
                {
                    Categoria = g.Key,
                    Total = g.Count(),
                    Percentual = chamados.Count > 0 ? (decimal)g.Count() / chamados.Count * 100 : 0
                })
                .ToList();

            // Volume por usuário
            relatorio.VolumePorUsuario = chamados
                .GroupBy(c => new { c.UsuarioId, c.Usuario!.Nome })
                .Select(g => new VolumePorUsuario
                {
                    UsuarioId = g.Key.UsuarioId,
                    UsuarioNome = g.Key.Nome,
                    TotalChamados = g.Count()
                })
                .OrderByDescending(v => v.TotalChamados)
                .ToList();

            // Volume por técnico
            relatorio.VolumePorTecnico = chamados
                .Where(c => c.TecnicoId.HasValue)
                .GroupBy(c => new { c.TecnicoId, c.Tecnico!.Nome })
                .Select(g => new VolumePorTecnico
                {
                    TecnicoId = g.Key.TecnicoId!.Value,
                    TecnicoNome = g.Key.Nome,
                    ChamadosAtribuidos = g.Count(),
                    ChamadosResolvidos = g.Count(c => c.Status == "Resolvido" || c.Status == "Fechado")
                })
                .ToList();

            return relatorio;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao gerar relatório de volume de {DataInicio} a {DataFim}: ex");
            throw;
        }
    }

    public async Task<RelatorioSatisfacao> GerarRelatorioSatisfacaoAsync(DateTime dataInicio, DateTime dataFim)
    {
        try
        {
            var chamados = await _context.Chamados
                .Include(c => c.Usuario)
                .Include(c => c.Tecnico)
                .Where(c => c.DataResolucao >= dataInicio && c.DataResolucao <= dataFim && c.Satisfacao.HasValue)
                .ToListAsync();

            var relatorio = new RelatorioSatisfacao
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalAvaliacoes = chamados.Count,
                SatisfacaoMedia = chamados.Average(c => (decimal)c.Satisfacao!.Value),
                Avaliacoes5 = chamados.Count(c => c.Satisfacao == 5),
                Avaliacoes4 = chamados.Count(c => c.Satisfacao == 4),
                Avaliacoes3 = chamados.Count(c => c.Satisfacao == 3),
                Avaliacoes2 = chamados.Count(c => c.Satisfacao == 2),
                Avaliacoes1 = chamados.Count(c => c.Satisfacao == 1)
            };

            // Satisfação por técnico
            relatorio.SatisfacaoPorTecnico = chamados
                .Where(c => c.TecnicoId.HasValue)
                .GroupBy(c => new { c.TecnicoId, c.Tecnico!.Nome })
                .Select(g => new SatisfacaoPorTecnico
                {
                    TecnicoId = g.Key.TecnicoId!.Value,
                    TecnicoNome = g.Key.Nome,
                    TotalAvaliacoes = g.Count(),
                    SatisfacaoMedia = g.Average(c => (decimal)c.Satisfacao!.Value)
                })
                .ToList();

            // Satisfação por categoria
            relatorio.SatisfacaoPorCategoria = chamados
                .Where(c => !string.IsNullOrEmpty(c.Categoria))
                .GroupBy(c => c.Categoria)
                .Select(g => new SatisfacaoPorCategoria
                {
                    Categoria = g.Key,
                    TotalAvaliacoes = g.Count(),
                    SatisfacaoMedia = g.Average(c => (decimal)c.Satisfacao!.Value)
                })
                .ToList();

            // Satisfação por período
            relatorio.SatisfacaoPorPeriodo = chamados
                .GroupBy(c => c.DataResolucao!.Value.Date)
                .Select(g => new SatisfacaoPorPeriodo
                {
                    Data = g.Key,
                    TotalAvaliacoes = g.Count(),
                    SatisfacaoMedia = g.Average(c => (decimal)c.Satisfacao!.Value)
                })
                .OrderBy(s => s.Data)
                .ToList();

            // Comentários de satisfação
            relatorio.Comentarios = chamados
                .Where(c => !string.IsNullOrEmpty(c.ComentarioSatisfacao))
                .Select(c => new ComentarioSatisfacao
                {
                    ChamadoId = c.Id,
                    UsuarioNome = c.Usuario?.Nome ?? "N/A",
                    Satisfacao = c.Satisfacao!.Value,
                    Comentario = c.ComentarioSatisfacao!,
                    DataAvaliacao = c.DataResolucao!.Value
                })
                .ToList();

            return relatorio;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao gerar relatório de satisfação de {DataInicio} a {DataFim}: ex");
            throw;
        }
    }

    public async Task<RelatorioCustomizado> GerarRelatorioCustomizadoAsync(Relatorio relatorio)
    {
        try
        {
            var relatorioCustomizado = new RelatorioCustomizado
            {
                RelatorioId = relatorio.Id,
                Nome = relatorio.Nome,
                DataInicio = relatorio.DataInicio,
                DataFim = relatorio.DataFim,
                Parametros = relatorio.ParametrosDict
            };

            // Aqui seria implementada a lógica específica baseada nos parâmetros
            // Por enquanto, retorna dados básicos
            relatorioCustomizado.Dados = new List<object>();
            relatorioCustomizado.Graficos = new List<GraficoData>();

            return relatorioCustomizado;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao gerar relatório customizado {RelatorioId}: ex");
            throw;
        }
    }

    #endregion

    #region Métricas e KPIs

    public async Task<Metrica> CalcularMetricasDiariasAsync(DateTime data)
    {
        try
        {
            var dataInicio = data.Date;
            var dataFim = dataInicio.AddDays(1);

            var chamados = await _context.Chamados
                .Where(c => c.DataCriacao >= dataInicio && c.DataCriacao < dataFim)
                .ToListAsync();

            var metrica = new Metrica
            {
                DataMetrica = data,
                HoraMetrica = DateTime.Now.TimeOfDay,
                TipoMetrica = "Diaria",
                ChamadosAbertos = chamados.Count(c => c.DataCriacao >= dataInicio && c.DataCriacao < dataFim),
                ChamadosResolvidos = chamados.Count(c => c.DataResolucao >= dataInicio && c.DataResolucao < dataFim),
                ChamadosFechados = chamados.Count(c => c.DataFechamento >= dataInicio && c.DataFechamento < dataFim),
                TempoMedioResolucao = chamados.Where(c => c.TempoResolucao.HasValue)
                    .Average(c => (decimal)c.TempoResolucao.Value),
                SatisfacaoMedia = chamados.Where(c => c.Satisfacao.HasValue)
                    .Average(c => (decimal)c.Satisfacao.Value),
                UsuariosAtivos = chamados.Select(c => c.UsuarioId).Distinct().Count(),
                TecnicosAtivos = chamados.Where(c => c.TecnicoId.HasValue)
                    .Select(c => c.TecnicoId!.Value).Distinct().Count(),
                DataCriacao = DateTime.Now
            };

            // Salvar métrica no banco
            _context.Metricas.Add(metrica);
            await _context.SaveChangesAsync();

            return metrica;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao calcular métricas diárias para {Data}: ex");
            throw;
        }
    }

    public async Task<List<Metrica>> CalcularMetricasPeriodoAsync(DateTime dataInicio, DateTime dataFim, string tipoMetrica)
    {
        return await _context.Metricas
            .Where(m => m.DataMetrica >= dataInicio && m.DataMetrica <= dataFim && m.TipoMetrica == tipoMetrica)
            .OrderBy(m => m.DataMetrica)
            .ToListAsync();
    }

    public async Task<DashboardMetrics> ObterMetricasDashboardAsync()
    {
        try
        {
            var hoje = DateTime.Now.Date;
            var ontem = hoje.AddDays(-1);
            var ultimos30Dias = hoje.AddDays(-30);

            var chamadosHoje = await _context.Chamados
                .Where(c => c.DataCriacao >= hoje)
                .ToListAsync();

            var chamadosOntem = await _context.Chamados
                .Where(c => c.DataCriacao >= ontem && c.DataCriacao < hoje)
                .ToListAsync();

            var chamados30Dias = await _context.Chamados
                .Where(c => c.DataCriacao >= ultimos30Dias)
                .ToListAsync();

            var dashboard = new DashboardMetrics
            {
                ChamadosAbertos = chamadosHoje.Count(c => c.Status == "Aberto"),
                ChamadosEmAndamento = chamadosHoje.Count(c => c.Status == "EmAndamento"),
                ChamadosResolvidosHoje = chamadosHoje.Count(c => c.Status == "Resolvido" || c.Status == "Fechado"),
                ChamadosAtrasados = chamadosHoje.Count(c => c.IsAtrasado),
                TempoMedioResolucao = chamados30Dias.Where(c => c.TempoResolucao.HasValue)
                    .Average(c => (decimal)c.TempoResolucao.Value) / 60, // converter para horas
                SatisfacaoMedia = chamados30Dias.Where(c => c.Satisfacao.HasValue)
                    .Average(c => (decimal)c.Satisfacao.Value),
                TaxaResolucaoPrimeiroContato = 0
            };

            // Chamados por hora (últimas 24 horas)
            dashboard.ChamadosPorHora = Enumerable.Range(0, 24)
                .Select(hora => new MetricaPorHora
                {
                    Hora = hora,
                    TotalChamados = chamadosHoje.Count(c => c.DataCriacao.Hour == hora)
                })
                .ToList();

            // Chamados por dia (últimos 30 dias)
            dashboard.ChamadosPorDia = Enumerable.Range(0, 30)
                .Select(dias => ultimos30Dias.AddDays(dias))
                .Select(data => new MetricaPorDia
                {
                    Data = data,
                    ChamadosAbertos = chamados30Dias.Count(c => c.DataCriacao.Date == data && c.Status == "Aberto"),
                    ChamadosResolvidos = chamados30Dias.Count(c => c.DataResolucao?.Date == data)
                })
                .ToList();

            // Chamados por categoria
            dashboard.ChamadosPorCategoria = chamados30Dias
                .Where(c => !string.IsNullOrEmpty(c.Categoria))
                .GroupBy(c => c.Categoria)
                .Select(g => new MetricaPorCategoria
                {
                    Categoria = g.Key,
                    Total = g.Count()
                })
                .ToList();

            // Chamados por prioridade
            dashboard.ChamadosPorPrioridade = chamados30Dias
                .GroupBy(c => c.Prioridade)
                .Select(g => new MetricaPorPrioridade
                {
                    Prioridade = g.Key,
                    Total = g.Count()
                })
                .ToList();

            return dashboard;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter métricas do dashboard: ex");
            throw;
        }
    }

    #endregion

    #region Agendamento

    public async Task<List<Relatorio>> ObterRelatoriosAgendadosAsync()
    {
        return await _context.Relatorios
            .Where(r => r.Agendado && r.Ativo && r.ProximaExecucao <= DateTime.Now)
            .ToListAsync();
    }

    public async Task ProcessarRelatoriosAgendadosAsync()
    {
        try
        {
            var relatoriosAgendados = await ObterRelatoriosAgendadosAsync();

            foreach (var relatorio in relatoriosAgendados)
            {
                try
                {
                    // Gerar relatório baseado no tipo
                    switch (relatorio.Tipo)
                    {
                        case "Performance":
                            await GerarRelatorioPerformanceAsync(relatorio.DataInicio, relatorio.DataFim);
                            break;
                        case "Volume":
                            await GerarRelatorioVolumeAsync(relatorio.DataInicio, relatorio.DataFim);
                            break;
                        case "Satisfacao":
                            await GerarRelatorioSatisfacaoAsync(relatorio.DataInicio, relatorio.DataFim);
                            break;
                    }

                    // Atualizar próxima execução
                    relatorio.UltimaExecucao = DateTime.Now;
                    relatorio.ProximaExecucao = relatorio.Frequencia switch
                    {
                        "Diario" => DateTime.Now.AddDays(1),
                        "Semanal" => DateTime.Now.AddDays(7),
                        "Mensal" => DateTime.Now.AddMonths(1),
                        _ => null
                    };

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao processar relatório agendado {RelatorioId}: ex");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao processar relatórios agendados: ex");
        }
    }

    public async Task AgendarRelatorioAsync(Relatorio relatorio)
    {
        try
        {
            relatorio.Agendado = true;
            relatorio.ProximaExecucao = relatorio.Frequencia switch
            {
                "Diario" => DateTime.Now.AddDays(1),
                "Semanal" => DateTime.Now.AddDays(7),
                "Mensal" => DateTime.Now.AddMonths(1),
                _ => null
            };

            if (relatorio.Id == 0)
                await CriarRelatorioAsync(relatorio);
            else
                await AtualizarRelatorioAsync(relatorio);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao agendar relatório {Nome}: ex");
            throw;
        }
    }

    #endregion

}
