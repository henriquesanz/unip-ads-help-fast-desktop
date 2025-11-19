using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace HelpFastDesktop.Presentation.Controllers;

public class FAQController : BaseController
{
    private readonly IFAQService _faqService;
    private readonly IChamadoService _chamadoService;
    private readonly ISessionService _sessionService;
    private FAQModel _model;

    public FAQController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _faqService = serviceProvider.GetRequiredService<IFAQService>();
        _chamadoService = serviceProvider.GetRequiredService<IChamadoService>();
        _sessionService = serviceProvider.GetRequiredService<ISessionService>();
        _model = new FAQModel();
    }

    public FAQModel GetModel() => _model;

    public async Task<Chamado?> ObterUltimoChamadoAbertoAsync()
    {
        try
        {
            var usuario = _sessionService.UsuarioLogado;
            if (usuario == null)
            {
                return null;
            }

            var chamados = await _chamadoService.ListarChamadosDoUsuarioAsync(usuario.Id);
            var ultimoChamadoAberto = chamados
                .Where(c => c.Status == "Aberto" || c.Status == "EmAndamento")
                .OrderByDescending(c => c.DataAbertura)
                .FirstOrDefault();

            return ultimoChamadoAberto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar último chamado: {ex.Message}");
            return null;
        }
    }

    public async System.Threading.Tasks.Task CarregarFAQsAsync()
    {
        try
        {
            _model.IsLoading = true;
            _model.ErrorMessage = string.Empty;

            var faqs = await _faqService.ListarAtivosAsync();
            
            _model.FAQs.Clear();
            foreach (var faq in faqs)
            {
                _model.FAQs.Add(faq);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao carregar FAQs: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public async System.Threading.Tasks.Task BuscarAsync(string termo)
    {
        try
        {
            _model.IsLoading = true;
            _model.ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(termo))
            {
                await CarregarFAQsAsync();
                return;
            }

            var faqs = await _faqService.BuscarPorTextoAsync(termo);
            
            _model.FAQs.Clear();
            foreach (var faq in faqs)
            {
                _model.FAQs.Add(faq);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao buscar FAQs: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }

    public void SelecionarFAQ(FAQItem faq)
    {
        _model.FAQSelecionado = faq;
        OnFAQSelecionado?.Invoke(faq);
    }

    public event Action<FAQItem>? OnFAQSelecionado;
}

public class FAQModel : INotifyPropertyChanged
{
    private ObservableCollection<FAQItem> _faqs = new();
    private FAQItem? _faqSelecionado;
    private string _termoBusca = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;
    private int? _faqExpandidoId = null;

    public ObservableCollection<FAQItem> FAQs
    {
        get => _faqs;
        set
        {
            _faqs = value;
            OnPropertyChanged();
        }
    }

    public FAQItem? FAQSelecionado
    {
        get => _faqSelecionado;
        set
        {
            _faqSelecionado = value;
            OnPropertyChanged();
        }
    }

    public string TermoBusca
    {
        get => _termoBusca;
        set
        {
            _termoBusca = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public int? FAQExpandidoId
    {
        get => _faqExpandidoId;
        set
        {
            _faqExpandidoId = value;
            OnPropertyChanged();
        }
    }

    public bool IsFAQExpandido(int faqId)
    {
        return _faqExpandidoId == faqId;
    }

    public void ToggleFAQExpandido(int faqId)
    {
        if (_faqExpandidoId == faqId)
        {
            FAQExpandidoId = null;
        }
        else
        {
            FAQExpandidoId = faqId;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

