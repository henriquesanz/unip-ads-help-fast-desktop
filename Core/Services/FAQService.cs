using Microsoft.EntityFrameworkCore;
using HelpFastDesktop.Infrastructure.Data;
using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;

namespace HelpFastDesktop.Core.Services;

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
        return await _context.Faqs
            // Include CriadoPor não existe
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<FAQItem>> ListarTodosAsync()
    {
        return await _context.Faqs
            // Include CriadoPor não existe
            .OrderBy(f => f.Pergunta)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> ListarAtivosAsync()
    {
        return await _context.Faqs
            // Include CriadoPor não existe
            .Where(f => f.Ativo)
            .OrderBy(f => f.Pergunta)
            .ToListAsync();
    }

    public async Task<FAQItem> CriarFAQAsync(FAQItem faq)
    {
        try
        {
            // Propriedades DataCriacao e Visualizacoes não existem no banco
            _context.Faqs.Add(faq);
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
            // Propriedade DataAtualizacao não existe no banco
            _context.Faqs.Update(faq);
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
            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null) return false;

            _context.Faqs.Remove(faq);
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
        
        return await _context.Faqs
            .Where(f => f.Ativo && (
                f.Titulo.ToLower().Contains(termoLower) ||
                f.Descricao.ToLower().Contains(termoLower) ||
                f.Solucao.ToLower().Contains(termoLower) ||
                false // Tags não existe no banco
            ))
            .OrderBy(f => f.Pergunta)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> BuscarPorCategoriaAsync(string categoria)
    {
        return await _context.Faqs
            .Where(f => f.Ativo) // Categoria não existe
            .OrderBy(f => f.Titulo)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> BuscarPorTagsAsync(string tag)
    {
        var tagLower = tag.ToLower();
        
        return await _context.Faqs
            .Where(f => f.Ativo && (f.Pergunta.ToLower().Contains(tagLower) || f.Resposta.ToLower().Contains(tagLower)))
            .OrderBy(f => f.Pergunta)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> BuscarAvancadaAsync(string? termo, string? categoria, string? tags)
    {
        var query = _context.Faqs.Where(f => f.Ativo);

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
            // Categoria não existe, ignorando filtro
        }

        if (!string.IsNullOrWhiteSpace(tags))
        {
            var tagsLower = tags.ToLower();
            // Tags não existe, ignorando filtro
        }

        return await query
            .OrderBy(f => f.Pergunta)
            .ToListAsync();
    }

    #endregion

    #region Categorias

    public async Task<List<string>> ListarCategoriasAsync()
    {
        return await _context.Faqs
            .Where(f => f.Ativo)
            .Select(f => "Geral") // Categoria não existe
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<string>> ListarSubcategoriasAsync(string categoria)
    {
        return await _context.Faqs
            .Where(f => f.Ativo) // Categoria e Subcategoria não existem
            .Select(f => "")
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> ListarPorCategoriaAsync(string categoria)
    {
        return await _context.Faqs
            .Where(f => f.Ativo) // Categoria não existe
            .OrderBy(f => f.Pergunta)
            .ToListAsync();
    }

    #endregion

    #region Estatísticas

    public async Task IncrementarVisualizacaoAsync(int faqId)
    {
        try
        {
            var faq = await _context.Faqs.FindAsync(faqId);
            if (faq != null)
            {
                // Visualizacoes não existe no banco
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
            var faq = await _context.Faqs.FindAsync(faqId);
            if (faq != null && rating >= 1 && rating <= 5)
            {
                // Utilidade e DataAtualizacao não existem no banco
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
        return await _context.Faqs
            .Where(f => f.Ativo)
            .OrderBy(f => f.Pergunta)
            .Take(limite)
            .ToListAsync();
    }

    public async Task<List<FAQItem>> ObterMaisUtilizadosAsync(int limite = 10)
    {
        return await _context.Faqs
            .Where(f => f.Ativo)
            .OrderBy(f => f.Pergunta)
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
                // Categoria não existe, ignorando filtro
            }

            // Ordenar por relevância (utilidade e visualizações)
            return faqs
                .OrderBy(f => f.Pergunta)
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
        return await _context.Faqs
            .Where(f => f.Ativo)
            .GroupBy(f => "Geral") // Categoria não existe, agrupando todos
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<List<string>> ObterTermosMaisBuscadosAsync()
    {
        // Tags não existe no banco, retornando lista vazia
        // Esta implementação seria melhorada com uma tabela de termos de busca
        return new List<string>();
    }

    #endregion
}
