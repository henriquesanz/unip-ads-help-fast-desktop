using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using System.IO;

namespace HelpFastDesktop.Core.Services;

public class RelatorioService : IRelatorioService
{
    private readonly ApplicationDbContext _context;
    private static bool _questPdfLicenseConfigured;

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
                    .Average(c => (decimal)c.TempoResolucao.Value.TotalHours), // converter para horas
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
                        .Average(c => (decimal)c.TempoResolucao.Value.TotalHours),
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
                        .Average(c => (decimal)c.TempoResolucao.Value.TotalHours),
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
                    TempoResolucao = c.TempoResolucao.HasValue ? (decimal)c.TempoResolucao.Value.TotalHours : 0,
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
                .Where(c => c.Satisfacao.HasValue) // ComentarioSatisfacao não existe no banco
                .Select(c => new ComentarioSatisfacao
                {
                    ChamadoId = c.Id,
                    UsuarioNome = c.Usuario?.Nome ?? "N/A",
                    Satisfacao = c.Satisfacao!.Value,
                    Comentario = "", // ComentarioSatisfacao não existe no banco
                    DataAvaliacao = c.DataResolucao ?? DateTime.Now
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
                    .Average(c => (decimal)c.TempoResolucao.Value.TotalHours),
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
                    .Average(c => (decimal)c.TempoResolucao.Value.TotalHours), // converter para horas
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

    #region Relatório Consolidado

    public async Task<RelatorioSistemaDto> GerarRelatorioSistemaAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        try
        {
            var inicio = dataInicio?.Date;
            var fim = dataFim?.Date;
            var fimInclusivo = fim?.AddDays(1).AddTicks(-1);

            var cargos = await _context.Cargos
                .OrderBy(c => c.Nome)
                .Select(c => new CargoRelatorioDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    TotalUsuarios = c.Usuarios.Count()
                })
                .ToListAsync();

            var usuariosEntidades = await _context.Usuarios
                .Include(u => u.Cargo)
                .OrderBy(u => u.Nome)
                .ToListAsync();

            var chamadosQuery = _context.Chamados
                .Include(c => c.Cliente)
                .Include(c => c.Tecnico)
                .AsQueryable();

            if (inicio.HasValue)
                chamadosQuery = chamadosQuery.Where(c => c.DataAbertura >= inicio.Value);
            if (fimInclusivo.HasValue)
                chamadosQuery = chamadosQuery.Where(c => c.DataAbertura <= fimInclusivo.Value);

            var chamadosEntidades = await chamadosQuery
                .OrderByDescending(c => c.DataAbertura)
                .ToListAsync();

            var chatsQuery = _context.Chats
                .Include(c => c.Remetente)
                .Include(c => c.Destinatario)
                .AsQueryable();

            if (inicio.HasValue)
                chatsQuery = chatsQuery.Where(c => c.DataEnvio >= inicio.Value);
            if (fimInclusivo.HasValue)
                chatsQuery = chatsQuery.Where(c => c.DataEnvio <= fimInclusivo.Value);

            var chatsEntidades = await chatsQuery
                .OrderByDescending(c => c.DataEnvio)
                .ToListAsync();

            var chatIaQuery = _context.ChatIaResults.AsQueryable();

            if (inicio.HasValue)
                chatIaQuery = chatIaQuery.Where(r => r.CreatedAt >= inicio.Value);
            if (fimInclusivo.HasValue)
                chatIaQuery = chatIaQuery.Where(r => r.CreatedAt <= fimInclusivo.Value);

            var chatIaEntidades = await chatIaQuery
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var historicosQuery = _context.HistoricoChamados
                .Include(h => h.Usuario)
                .AsQueryable();

            if (inicio.HasValue)
                historicosQuery = historicosQuery.Where(h => h.Data >= inicio.Value);
            if (fimInclusivo.HasValue)
                historicosQuery = historicosQuery.Where(h => h.Data <= fimInclusivo.Value);

            var historicosEntidades = await historicosQuery
                .OrderByDescending(h => h.Data)
                .ToListAsync();

            var faqs = await _context.Faqs
                .OrderBy(f => f.Pergunta)
                .Select(f => new FaqRelatorioDto
                {
                    Id = f.Id,
                    Pergunta = f.Pergunta,
                    Ativo = f.Ativo
                })
                .ToListAsync();

            var chamados = chamadosEntidades
                .Select(c => new ChamadoRelatorioDto
                {
                    Id = c.Id,
                    Motivo = c.Motivo,
                    ClienteNome = c.Cliente?.Nome ?? "N/A",
                    TecnicoNome = c.Tecnico?.Nome,
                    Status = c.Status,
                    DataAbertura = c.DataAbertura,
                    DataFechamento = c.DataFechamento
                })
                .ToList();

            var chats = chatsEntidades
                .Select(c => new ChatRelatorioDto
                {
                    Id = c.Id,
                    ChamadoId = c.ChamadoId,
                    RemetenteNome = c.Remetente?.Nome ?? "N/A",
                    DestinatarioNome = c.Destinatario?.Nome,
                    Tipo = c.Tipo,
                    DataEnvio = c.DataEnvio,
                    Mensagem = c.Mensagem
                })
                .ToList();

            var chatIaResults = chatIaEntidades
                .Select(r => new ChatIaRelatorioDto
                {
                    Id = r.Id,
                    ChatId = r.ChatId,
                    CreatedAt = r.CreatedAt,
                    Resumo = CriarResumoJson(r.ResultJson)
                })
                .ToList();

            var historicos = historicosEntidades
                .Select(h => new HistoricoChamadoRelatorioDto
                {
                    Id = h.Id,
                    ChamadoId = h.ChamadoId,
                    Acao = h.Acao,
                    UsuarioNome = h.Usuario?.Nome ?? "N/A",
                    Data = h.Data
                })
                .ToList();

            var usuarios = usuariosEntidades
                .Select(u =>
                {
                    var chamadosUsuario = chamadosEntidades.Where(c => c.ClienteId == u.Id).ToList();
                    return new UsuarioRelatorioDto
                    {
                        Id = u.Id,
                        Nome = u.Nome,
                        Email = u.Email,
                        CargoNome = u.Cargo?.Nome ?? "Sem cargo",
                        Telefone = u.Telefone,
                        UltimoLogin = u.UltimoLogin,
                        ChamadosTotais = chamadosUsuario.Count,
                        ChamadosEmAberto = chamadosUsuario.Count(c => c.Status == "Aberto" || c.Status == "EmAndamento"),
                        ChamadosResolvidos = chamadosUsuario.Count(c => c.Status == "Resolvido" || c.Status == "Fechado")
                    };
                })
                .OrderBy(u => u.Nome)
                .ToList();

            var resumo = new RelatorioSistemaResumoDto
            {
                TotalCargos = cargos.Count,
                TotalUsuarios = usuariosEntidades.Count,
                TotalChamados = chamadosEntidades.Count,
                ChamadosAbertos = chamadosEntidades.Count(c => c.Status == "Aberto"),
                ChamadosEmAndamento = chamadosEntidades.Count(c => c.Status == "EmAndamento"),
                ChamadosResolvidos = chamadosEntidades.Count(c => c.Status == "Resolvido"),
                ChamadosFechados = chamadosEntidades.Count(c => c.Status == "Fechado"),
                TotalChats = chatsEntidades.Count,
                TotalChatIaResultados = chatIaEntidades.Count,
                TotalHistoricoChamados = historicosEntidades.Count,
                TotalFaqs = faqs.Count,
                FaqsAtivas = faqs.Count(f => f.Ativo),
                FaqsInativas = faqs.Count(f => !f.Ativo)
            };

            return new RelatorioSistemaDto
            {
                GeradoEm = DateTime.Now,
                PeriodoInicio = inicio,
                PeriodoFim = fim,
                Resumo = resumo,
                Cargos = cargos,
                Usuarios = usuarios,
                Chamados = chamados,
                Chats = chats,
                ChatIaResults = chatIaResults,
                HistoricosChamados = historicos,
                Faqs = faqs
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao gerar relatório consolidado: {ex}");
            throw;
        }
    }

    public Task<byte[]> ExportarRelatorioSistemaAsync(RelatorioSistemaDto relatorio, RelatorioFormatoExportacao formato)
    {
        if (relatorio == null)
            throw new ArgumentNullException(nameof(relatorio));

        return formato switch
        {
            RelatorioFormatoExportacao.Pdf => Task.FromResult(GerarPdf(relatorio)),
            RelatorioFormatoExportacao.Excel => Task.FromResult(GerarExcel(relatorio)),
            _ => throw new NotSupportedException($"Formato de exportação não suportado: {formato}")
        };
    }

    private static byte[] GerarExcel(RelatorioSistemaDto relatorio)
    {
        using var workbook = new XLWorkbook();

        var resumoSheet = workbook.Worksheets.Add("Resumo");
        resumoSheet.Cell(1, 1).Value = "Indicador";
        resumoSheet.Cell(1, 2).Value = "Valor";

        var linha = 2;

        void AdicionarResumo(string indicador, object valor)
        {
            resumoSheet.Cell(linha, 1).SetValue(indicador ?? string.Empty);

            if (valor == null)
            {
                resumoSheet.Cell(linha, 2).SetValue(string.Empty);
            }
            else
            {
                switch (valor)
                {
                    case int i:
                        resumoSheet.Cell(linha, 2).SetValue(i);
                        break;
                    case long l:
                        resumoSheet.Cell(linha, 2).SetValue(l);
                        break;
                    case decimal dec:
                        resumoSheet.Cell(linha, 2).SetValue(dec);
                        break;
                    case double dbl:
                        resumoSheet.Cell(linha, 2).SetValue(dbl);
                        break;
                    case float flt:
                        resumoSheet.Cell(linha, 2).SetValue(flt);
                        break;
                    case DateTime dateTime:
                        resumoSheet.Cell(linha, 2).SetValue(dateTime);
                        break;
                    case bool boolean:
                        resumoSheet.Cell(linha, 2).SetValue(boolean);
                        break;
                    default:
                        resumoSheet.Cell(linha, 2).SetValue(valor.ToString() ?? string.Empty);
                        break;
                }
            }
            linha++;
        }

        var resumo = relatorio.Resumo;
        AdicionarResumo("Total de cargos", resumo.TotalCargos);
        AdicionarResumo("Total de usuários", resumo.TotalUsuarios);
        AdicionarResumo("Total de chamados", resumo.TotalChamados);
        AdicionarResumo("Chamados abertos", resumo.ChamadosAbertos);
        AdicionarResumo("Chamados em andamento", resumo.ChamadosEmAndamento);
        AdicionarResumo("Chamados resolvidos", resumo.ChamadosResolvidos);
        AdicionarResumo("Chamados fechados", resumo.ChamadosFechados);
        AdicionarResumo("Mensagens de chat", resumo.TotalChats);
        AdicionarResumo("Respostas IA", resumo.TotalChatIaResultados);
        AdicionarResumo("Eventos de histórico", resumo.TotalHistoricoChamados);
        AdicionarResumo("FAQs ativas", resumo.FaqsAtivas);
        AdicionarResumo("FAQs inativas", resumo.FaqsInativas);

        resumoSheet.Columns().AdjustToContents();

        CriarPlanilha(workbook, "Cargos", new[] { "Id", "Nome", "Total usuários" }, relatorio.Cargos, (planilha, item, row) =>
        {
            planilha.Cell(row, 1).Value = item.Id;
            planilha.Cell(row, 2).Value = item.Nome;
            planilha.Cell(row, 3).Value = item.TotalUsuarios;
        });

        CriarPlanilha(workbook, "Usuarios", new[] { "Id", "Nome", "Email", "Cargo", "Telefone", "Último login", "Chamados totais", "Chamados em aberto", "Chamados resolvidos" }, relatorio.Usuarios, (planilha, item, row) =>
        {
            planilha.Cell(row, 1).Value = item.Id;
            planilha.Cell(row, 2).Value = item.Nome;
            planilha.Cell(row, 3).Value = item.Email;
            planilha.Cell(row, 4).Value = item.CargoNome;
            planilha.Cell(row, 5).Value = item.Telefone ?? "-";
            planilha.Cell(row, 6).Value = item.UltimoLogin?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            planilha.Cell(row, 7).Value = item.ChamadosTotais;
            planilha.Cell(row, 8).Value = item.ChamadosEmAberto;
            planilha.Cell(row, 9).Value = item.ChamadosResolvidos;
        });

        CriarPlanilha(workbook, "Chamados", new[] { "Id", "Cliente", "Técnico", "Status", "Data abertura", "Data fechamento", "Motivo" }, relatorio.Chamados, (planilha, item, row) =>
        {
            planilha.Cell(row, 1).Value = item.Id;
            planilha.Cell(row, 2).Value = item.ClienteNome;
            planilha.Cell(row, 3).Value = item.TecnicoNome ?? "-";
            planilha.Cell(row, 4).Value = item.Status;
            planilha.Cell(row, 5).Value = item.DataAbertura.ToString("dd/MM/yyyy HH:mm");
            planilha.Cell(row, 6).Value = item.DataFechamento?.ToString("dd/MM/yyyy HH:mm") ?? "-";
            planilha.Cell(row, 7).Value = item.Motivo;
        });

        CriarPlanilha(workbook, "Chats", new[] { "Id", "Chamado", "Remetente", "Destinatário", "Tipo", "Data envio", "Mensagem" }, relatorio.Chats, (planilha, item, row) =>
        {
            planilha.Cell(row, 1).Value = item.Id;
            planilha.Cell(row, 2).Value = item.ChamadoId.HasValue ? item.ChamadoId.Value.ToString() : "-";
            planilha.Cell(row, 3).Value = item.RemetenteNome;
            planilha.Cell(row, 4).Value = item.DestinatarioNome ?? "-";
            planilha.Cell(row, 5).Value = item.Tipo ?? "-";
            planilha.Cell(row, 6).Value = item.DataEnvio.ToString("dd/MM/yyyy HH:mm");
            planilha.Cell(row, 7).Value = item.Mensagem;
        });

        CriarPlanilha(workbook, "ChatIA", new[] { "Id", "Chat", "Criado em", "Resumo" }, relatorio.ChatIaResults, (planilha, item, row) =>
        {
            planilha.Cell(row, 1).Value = item.Id;
            planilha.Cell(row, 2).Value = item.ChatId;
            planilha.Cell(row, 3).Value = item.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            planilha.Cell(row, 4).Value = item.Resumo;
        });

        CriarPlanilha(workbook, "HistoricoChamados", new[] { "Id", "Chamado", "Ação", "Usuário", "Data" }, relatorio.HistoricosChamados, (planilha, item, row) =>
        {
            planilha.Cell(row, 1).Value = item.Id;
            planilha.Cell(row, 2).Value = item.ChamadoId;
            planilha.Cell(row, 3).Value = item.Acao;
            planilha.Cell(row, 4).Value = item.UsuarioNome;
            planilha.Cell(row, 5).Value = item.Data.ToString("dd/MM/yyyy HH:mm");
        });

        CriarPlanilha(workbook, "Faqs", new[] { "Id", "Pergunta", "Ativo" }, relatorio.Faqs, (planilha, item, row) =>
        {
            planilha.Cell(row, 1).Value = item.Id;
            planilha.Cell(row, 2).Value = item.Pergunta;
            planilha.Cell(row, 3).Value = item.Ativo ? "Sim" : "Não";
        });

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void CriarPlanilha<T>(XLWorkbook workbook, string nome, string[] cabecalhos, IReadOnlyCollection<T> dados, Action<IXLWorksheet, T, int> preencherLinha)
    {
        var planilha = workbook.Worksheets.Add(nome);

        for (var i = 0; i < cabecalhos.Length; i++)
        {
            planilha.Cell(1, i + 1).Value = cabecalhos[i];
            planilha.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var linha = 2;
        foreach (var item in dados)
        {
            preencherLinha(planilha, item, linha);
            linha++;
        }

        planilha.Columns().AdjustToContents();
    }

    private static byte[] GerarPdf(RelatorioSistemaDto relatorio)
    {
        EnsureQuestPdfLicense();
        var documento = new RelatorioSistemaPdfDocument(relatorio);
        return documento.GeneratePdf();
    }

    private static void EnsureQuestPdfLicense()
    {
        if (_questPdfLicenseConfigured)
            return;

        QuestPDF.Settings.License = LicenseType.Community;
        _questPdfLicenseConfigured = true;
    }

    private static string CriarResumoJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        try
        {
            using var document = JsonDocument.Parse(json);
            var texto = JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            return Truncar(texto, 200);
        }
        catch
        {
            return Truncar(json, 200);
        }
    }

    private static string Truncar(string valor, int tamanhoMaximo)
    {
        if (string.IsNullOrEmpty(valor) || valor.Length <= tamanhoMaximo)
            return valor;

        return valor.Substring(0, tamanhoMaximo) + "...";
    }

    private sealed class RelatorioSistemaPdfDocument : IDocument
    {
        private readonly RelatorioSistemaDto _relatorio;

        public RelatorioSistemaPdfDocument(RelatorioSistemaDto relatorio)
        {
            _relatorio = relatorio;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Column(column =>
                {
                    column.Item().Text("Relatório Consolidado de Operações")
                        .FontSize(20).SemiBold();

                    column.Item().Text(text =>
                    {
                        text.Span("Gerado em: ").SemiBold();
                        text.Span(_relatorio.GeradoEm.ToString("dd/MM/yyyy HH:mm"));
                    });

                    if (_relatorio.PeriodoInicio.HasValue || _relatorio.PeriodoFim.HasValue)
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Período: ").SemiBold();
                            text.Span($"{FormatarData(_relatorio.PeriodoInicio)} - {FormatarData(_relatorio.PeriodoFim)}");
                        });
                    }
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Element(ComposeResumo);
                    column.Item().Element(ComposeCargos);
                    column.Item().Element(ComposeUsuarios);
                    column.Item().Element(ComposeChamados);
                    column.Item().Element(ComposeChats);
                    column.Item().Element(ComposeChatIa);
                    column.Item().Element(ComposeHistoricos);
                    column.Item().Element(ComposeFaqs);
                });

                page.Footer()
                    .AlignCenter()
                    .Text("HelpFast Desktop - Relatório Consolidado")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });
        }

        private void ComposeResumo(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(4);
                column.Item().Text("Resumo Executivo").FontSize(14).SemiBold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeader).Text("Indicador");
                        header.Cell().Element(CellHeader).Text("Valor");
                    });

                    void Linha(string indicador, string valor)
                    {
                        table.Cell().Element(CellBody).Text(indicador);
                        table.Cell().Element(CellBody).Text(valor);
                    }

                    var resumo = _relatorio.Resumo;
                    Linha("Total de cargos", resumo.TotalCargos.ToString());
                    Linha("Total de usuários", resumo.TotalUsuarios.ToString());
                    Linha("Total de chamados", resumo.TotalChamados.ToString());
                    Linha("Chamados abertos", resumo.ChamadosAbertos.ToString());
                    Linha("Chamados em andamento", resumo.ChamadosEmAndamento.ToString());
                    Linha("Chamados resolvidos", resumo.ChamadosResolvidos.ToString());
                    Linha("Chamados fechados", resumo.ChamadosFechados.ToString());
                    Linha("Mensagens de chat", resumo.TotalChats.ToString());
                    Linha("Respostas IA", resumo.TotalChatIaResultados.ToString());
                    Linha("Eventos de histórico", resumo.TotalHistoricoChamados.ToString());
                    Linha("FAQs ativas", resumo.FaqsAtivas.ToString());
                    Linha("FAQs inativas", resumo.FaqsInativas.ToString());
                });
            });
        }

        private void ComposeCargos(IContainer container)
        {
            ComposeTabela(container, "Cargos", _relatorio.Cargos, (table, dados) =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn();
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Id");
                    header.Cell().Element(CellHeader).Text("Nome");
                    header.Cell().Element(CellHeader).Text("Usuários");
                });

                foreach (var cargo in dados)
                {
                    table.Cell().Element(CellBody).Text(cargo.Id.ToString());
                    table.Cell().Element(CellBody).Text(cargo.Nome);
                    table.Cell().Element(CellBody).Text(cargo.TotalUsuarios.ToString());
                }
            });
        }

        private void ComposeUsuarios(IContainer container)
        {
            ComposeTabela(container, "Usuários", _relatorio.Usuarios, (table, dados) =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Id");
                    header.Cell().Element(CellHeader).Text("Nome");
                    header.Cell().Element(CellHeader).Text("Email");
                    header.Cell().Element(CellHeader).Text("Cargo");
                    header.Cell().Element(CellHeader).Text("Tickets");
                    header.Cell().Element(CellHeader).Text("Em aberto");
                    header.Cell().Element(CellHeader).Text("Resolvidos");
                });

                foreach (var usuario in dados)
                {
                    table.Cell().Element(CellBody).Text(usuario.Id.ToString());
                    table.Cell().Element(CellBody).Text(usuario.Nome);
                    table.Cell().Element(CellBody).Text(usuario.Email);
                    table.Cell().Element(CellBody).Text(usuario.CargoNome);
                    table.Cell().Element(CellBody).Text(usuario.ChamadosTotais.ToString());
                    table.Cell().Element(CellBody).Text(usuario.ChamadosEmAberto.ToString());
                    table.Cell().Element(CellBody).Text(usuario.ChamadosResolvidos.ToString());
                }
            });
        }

        private void ComposeChamados(IContainer container)
        {
            ComposeTabela(container, "Chamados", _relatorio.Chamados, (table, dados) =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(90);
                    columns.ConstantColumn(90);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Id");
                    header.Cell().Element(CellHeader).Text("Cliente");
                    header.Cell().Element(CellHeader).Text("Técnico");
                    header.Cell().Element(CellHeader).Text("Status");
                    header.Cell().Element(CellHeader).Text("Abertura");
                    header.Cell().Element(CellHeader).Text("Fechamento");
                    header.Cell().Element(CellHeader).Text("Motivo");
                });

                foreach (var chamado in dados)
                {
                    table.Cell().Element(CellBody).Text(chamado.Id.ToString());
                    table.Cell().Element(CellBody).Text(chamado.ClienteNome);
                    table.Cell().Element(CellBody).Text(chamado.TecnicoNome ?? "-");
                    table.Cell().Element(CellBody).Text(chamado.Status);
                    table.Cell().Element(CellBody).Text(chamado.DataAbertura.ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(CellBody).Text(chamado.DataFechamento?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                    table.Cell().Element(CellBody).Text(Truncar(chamado.Motivo, 120));
                }
            });
        }

        private void ComposeChats(IContainer container)
        {
            ComposeTabela(container, "Chats", _relatorio.Chats, (table, dados) =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.ConstantColumn(45);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(90);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Id");
                    header.Cell().Element(CellHeader).Text("Chamado");
                    header.Cell().Element(CellHeader).Text("Remetente");
                    header.Cell().Element(CellHeader).Text("Destinatário");
                    header.Cell().Element(CellHeader).Text("Tipo");
                    header.Cell().Element(CellHeader).Text("Data");
                    header.Cell().Element(CellHeader).Text("Mensagem");
                });

                foreach (var chat in dados)
                {
                    table.Cell().Element(CellBody).Text(chat.Id.ToString());
                    table.Cell().Element(CellBody).Text(chat.ChamadoId?.ToString() ?? "-");
                    table.Cell().Element(CellBody).Text(chat.RemetenteNome);
                    table.Cell().Element(CellBody).Text(chat.DestinatarioNome ?? "-");
                    table.Cell().Element(CellBody).Text(chat.Tipo ?? "-");
                    table.Cell().Element(CellBody).Text(chat.DataEnvio.ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(CellBody).Text(Truncar(chat.Mensagem, 100));
                }
            });
        }

        private void ComposeChatIa(IContainer container)
        {
            ComposeTabela(container, "Resultados da IA", _relatorio.ChatIaResults, (table, dados) =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.ConstantColumn(45);
                    columns.ConstantColumn(110);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Id");
                    header.Cell().Element(CellHeader).Text("Chat");
                    header.Cell().Element(CellHeader).Text("Criado em");
                    header.Cell().Element(CellHeader).Text("Resumo");
                });

                foreach (var item in dados)
                {
                    table.Cell().Element(CellBody).Text(item.Id.ToString());
                    table.Cell().Element(CellBody).Text(item.ChatId.ToString());
                    table.Cell().Element(CellBody).Text(item.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(CellBody).Text(item.Resumo);
                }
            });
        }

        private void ComposeHistoricos(IContainer container)
        {
            ComposeTabela(container, "Histórico de chamados", _relatorio.HistoricosChamados, (table, dados) =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.ConstantColumn(45);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(110);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Id");
                    header.Cell().Element(CellHeader).Text("Chamado");
                    header.Cell().Element(CellHeader).Text("Ação");
                    header.Cell().Element(CellHeader).Text("Usuário");
                    header.Cell().Element(CellHeader).Text("Data");
                });

                foreach (var item in dados)
                {
                    table.Cell().Element(CellBody).Text(item.Id.ToString());
                    table.Cell().Element(CellBody).Text(item.ChamadoId.ToString());
                    table.Cell().Element(CellBody).Text(item.Acao);
                    table.Cell().Element(CellBody).Text(item.UsuarioNome);
                    table.Cell().Element(CellBody).Text(item.Data.ToString("dd/MM/yyyy HH:mm"));
                }
            });
        }

        private void ComposeFaqs(IContainer container)
        {
            ComposeTabela(container, "FAQs", _relatorio.Faqs, (table, dados) =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn();
                    columns.ConstantColumn(60);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Id");
                    header.Cell().Element(CellHeader).Text("Pergunta");
                    header.Cell().Element(CellHeader).Text("Ativa");
                });

                foreach (var faq in dados)
                {
                    table.Cell().Element(CellBody).Text(faq.Id.ToString());
                    table.Cell().Element(CellBody).Text(Truncar(faq.Pergunta, 120));
                    table.Cell().Element(CellBody).Text(faq.Ativo ? "Sim" : "Não");
                }
            });
        }

        private void ComposeTabela<T>(IContainer container, string titulo, IReadOnlyCollection<T> dados, Action<TableDescriptor, IReadOnlyCollection<T>> conteudo)
        {
            container.Column(column =>
            {
                column.Spacing(4);
                column.Item().Text(titulo).FontSize(14).SemiBold();

                if (dados.Count == 0)
                {
                    column.Item()
                        .Text("Nenhum registro encontrado no período.")
                        .FontSize(10)
                        .Italic()
                        .FontColor(Colors.Grey.Darken1);
                    return;
                }

                column.Item().Table(table => conteudo(table, dados));
            });
        }

        private static IContainer CellHeader(IContainer container) =>
            container.Border(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten3)
                .Padding(4)
                .DefaultTextStyle(x => x.SemiBold());

        private static IContainer CellBody(IContainer container) =>
            container.BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(4);

        private static string FormatarData(DateTime? data) =>
            data.HasValue ? data.Value.ToString("dd/MM/yyyy") : "-";
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
