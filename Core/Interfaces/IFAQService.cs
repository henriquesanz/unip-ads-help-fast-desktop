using HelpFastDesktop.Core.Entities;

namespace HelpFastDesktop.Core.Interfaces;

public interface IFAQService
{
    // CRUD básico
    Task<FAQItem?> ObterPorIdAsync(int id);
    Task<List<FAQItem>> ListarTodosAsync();
    Task<List<FAQItem>> ListarAtivosAsync();
    Task<FAQItem> CriarFAQAsync(FAQItem faq);
    Task<FAQItem> AtualizarFAQAsync(FAQItem faq);
    Task<bool> ExcluirFAQAsync(int id);

    // Busca e filtros
    Task<List<FAQItem>> BuscarPorTextoAsync(string termo);
    Task<List<FAQItem>> BuscarPorCategoriaAsync(string categoria);
    Task<List<FAQItem>> BuscarPorTagsAsync(string tag);
    Task<List<FAQItem>> BuscarAvancadaAsync(string? termo, string? categoria, string? tags);

    // Categorias
    Task<List<string>> ListarCategoriasAsync();
    Task<List<string>> ListarSubcategoriasAsync(string categoria);
    Task<List<FAQItem>> ListarPorCategoriaAsync(string categoria);

    // Estatísticas
    Task IncrementarVisualizacaoAsync(int faqId);
    Task AtualizarUtilidadeAsync(int faqId, int rating);
    Task<List<FAQItem>> ObterMaisVisualizadosAsync(int limite = 10);
    Task<List<FAQItem>> ObterMaisUtilizadosAsync(int limite = 10);

    // Sugestões baseadas em IA
    Task<List<string>> SugerirFAQParaChamadoAsync(Chamado chamado);
    Task<List<FAQItem>> ObterFAQsRelevantesAsync(string descricao, string? categoria = null);

    // Relatórios
    Task<Dictionary<string, int>> ObterEstatisticasPorCategoriaAsync();
    Task<List<string>> ObterTermosMaisBuscadosAsync();
}
