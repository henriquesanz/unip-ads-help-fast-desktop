using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Data;
using HelpFastDesktop.Models;

namespace HelpFastDesktop.Services;

public class FAQService : IFAQService
{
    private readonly ApplicationDbContext _context;
    private readonly IAIService _aiService;

    public FAQService(ApplicationDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    #region CRUD Básico

    public async Task<FAQItem?> ObterPorIdAsync(int id)
    {
        return await _context.FAQ
            .Include(f => f.CriadoPor)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<FAQItem>> ListarTodosAsync()
    {
        return await _context.FAQ
            .Include(f => f.CriadoPor)
            .OrderBy(f => f.Categoria)
            .ThenBy(f => f.Titulo)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> ListarAtivosAsync()
    {
        return await _context.FAQ
            .Include(f => f.CriadoPor)
            .Where(f => f.Ativo)
            .OrderBy(f => f.Categoria)
            .ThenBy(f => f.Titulo)
            .ToListAsync();
    }

    public async Task<FAQItem> CriarFAQAsync(FAQItem faq)
    {
        try
        {
            faq.DataCriacao = DateTime.Now;
            faq.Visualizacoes = 0;
            
            _context.FAQ.Add(faq);
            await _context.SaveChangesAsync();
            
            return faq;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao criar FAQ: {Titulo}: ex");
            throw;
        }
    }

    public async Task<FAQItem> AtualizarFAQAsync(FAQItem faq)
    {
        try
        {
            faq.DataAtualizacao = DateTime.Now;
            _context.FAQ.Update(faq);
            await _context.SaveChangesAsync();
            
            return faq;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao atualizar FAQ {FAQId}: ex");
            throw;
        }
    }

    public async Task<bool> ExcluirFAQAsync(int id)
    {
        try
        {
            var faq = await _context.FAQ.FindAsync(id);
            if (faq == null) return false;

            _context.FAQ.Remove(faq);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao excluir FAQ {FAQId}: ex");
            return false;
        }
    }

    #endregion

    #region Busca e Filtros

    public async Task<List<FAQItem>> BuscarPorTextoAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return new List<FAQItem>();

        var termoLower = termo.ToLower();
        
        return await _context.FAQ
            .Where(f => f.Ativo && (
                f.Titulo.ToLower().Contains(termoLower) ||
                f.Descricao.ToLower().Contains(termoLower) ||
                f.Solucao.ToLower().Contains(termoLower) ||
                f.Tags.ToLower().Contains(termoLower)
            ))
            .OrderByDescending(f => f.Visualizacoes)
            .ThenByDescending(f => f.Utilidade)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> BuscarPorCategoriaAsync(string categoria)
    {
        return await _context.FAQ
            .Where(f => f.Ativo && f.Categoria == categoria)
            .OrderBy(f => f.Titulo)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> BuscarPorTagsAsync(string tag)
    {
        var tagLower = tag.ToLower();
        
        return await _context.FAQ
            .Where(f => f.Ativo && f.Tags.ToLower().Contains(tagLower))
            .OrderByDescending(f => f.Visualizacoes)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> BuscarAvancadaAsync(string? termo, string? categoria, string? tags)
    {
        var query = _context.FAQ.Where(f => f.Ativo);

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var termoLower = termo.ToLower();
            query = query.Where(f => 
                f.Titulo.ToLower().Contains(termoLower) ||
                f.Descricao.ToLower().Contains(termoLower) ||
                f.Solucao.ToLower().Contains(termoLower));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = query.Where(f => f.Categoria == categoria);
        }

        if (!string.IsNullOrWhiteSpace(tags))
        {
            var tagsLower = tags.ToLower();
            query = query.Where(f => f.Tags.ToLower().Contains(tagsLower));
        }

        return await query
            .OrderByDescending(f => f.Visualizacoes)
            .ThenByDescending(f => f.Utilidade)
            .ToListAsync();
    }

    #endregion

    #region Categorias

    public async Task<List<string>> ListarCategoriasAsync()
    {
        return await _context.FAQ
            .Where(f => f.Ativo)
            .Select(f => f.Categoria)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<string>> ListarSubcategoriasAsync(string categoria)
    {
        return await _context.FAQ
            .Where(f => f.Ativo && f.Categoria == categoria && !string.IsNullOrEmpty(f.Subcategoria))
            .Select(f => f.Subcategoria!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> ListarPorCategoriaAsync(string categoria)
    {
        return await _context.FAQ
            .Where(f => f.Ativo && f.Categoria == categoria)
            .OrderBy(f => f.Subcategoria)
            .ThenBy(f => f.Titulo)
            .ToListAsync();
    }

    #endregion

    #region Estatísticas

    public async Task IncrementarVisualizacaoAsync(int faqId)
    {
        try
        {
            var faq = await _context.FAQ.FindAsync(faqId);
            if (faq != null)
            {
                faq.Visualizacoes++;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao incrementar visualização do FAQ {FAQId}: ex");
        }
    }

    public async Task AtualizarUtilidadeAsync(int faqId, int rating)
    {
        try
        {
            var faq = await _context.FAQ.FindAsync(faqId);
            if (faq != null && rating >= 1 && rating <= 5)
            {
                faq.Utilidade = rating;
                faq.DataAtualizacao = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao atualizar utilidade do FAQ {FAQId}: ex");
        }
    }

    public async Task<List<FAQItem>> ObterMaisVisualizadosAsync(int limite = 10)
    {
        return await _context.FAQ
            .Where(f => f.Ativo)
            .OrderByDescending(f => f.Visualizacoes)
            .Take(limite)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> ObterMaisUtilizadosAsync(int limite = 10)
    {
        return await _context.FAQ
            .Where(f => f.Ativo && f.Utilidade.HasValue)
            .OrderByDescending(f => f.Utilidade)
            .ThenByDescending(f => f.Visualizacoes)
            .Take(limite)
            .ToListAsync();
    }

    #endregion

    #region Sugestões baseadas em IA

    public async Task<List<string>> SugerirFAQParaChamadoAsync(Chamado chamado)
    {
        try
        {
            var sugestoes = await _aiService.SugerirFAQAsync(chamado.Descricao, chamado.Categoria);
            return sugestoes;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao sugerir FAQ para chamado {ChamadoId}: ex");
            return new List<string>();
        }
    }

    public async Task<List<FAQItem>> ObterFAQsRelevantesAsync(string descricao, string? categoria = null)
    {
        try
        {
            // Buscar FAQs relevantes baseado na descrição
            var faqs = await BuscarPorTextoAsync(descricao);
            
            // Filtrar por categoria se especificada
            if (!string.IsNullOrEmpty(categoria))
            {
                faqs = faqs.Where(f => f.Categoria == categoria).ToList();
            }

            // Ordenar por relevância (utilidade e visualizações)
            return faqs
                .OrderByDescending(f => f.Utilidade ?? 0)
                .ThenByDescending(f => f.Visualizacoes)
                .Take(5)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter FAQs relevantes para descrição: ex");
            return new List<FAQItem>();
        }
    }

    #endregion

    #region Relatórios

    public async Task<Dictionary<string, int>> ObterEstatisticasPorCategoriaAsync()
    {
        return await _context.FAQ
            .Where(f => f.Ativo)
            .GroupBy(f => f.Categoria)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<List<string>> ObterTermosMaisBuscadosAsync()
    {
        // Esta implementação seria melhorada com uma tabela de termos de busca
        // Por enquanto, retorna as tags mais comuns
        var todasTags = await _context.FAQ
            .Where(f => f.Ativo && !string.IsNullOrEmpty(f.Tags))
            .Select(f => f.Tags)
            .ToListAsync();

        var contadorTags = new Dictionary<string, int>();
        
        foreach (var tagString in todasTags)
        {
            var tags = tagString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
            {
                var tagTrimmed = tag.Trim().ToLower();
                if (!string.IsNullOrEmpty(tagTrimmed))
                {
                    if (contadorTags.ContainsKey(tagTrimmed))
                        contadorTags[tagTrimmed]++;
                    else
                        contadorTags[tagTrimmed] = 1;
                }
            }
        }

        return contadorTags
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    #endregion
}
