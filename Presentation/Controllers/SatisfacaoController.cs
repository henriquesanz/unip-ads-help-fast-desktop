using HelpFastDesktop.Core.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class SatisfacaoController : BaseController
{
    private SatisfacaoModel _model;

    public SatisfacaoController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _model = new SatisfacaoModel();
    }

    public SatisfacaoModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarSatisfacaoAsync()
    {
        try
        {
            _model.IsLoading = true;
            _model.Mensagem = "Análise de Satisfação:\n\n- Taxa de satisfação geral\n- Satisfação por técnico\n- Tendências de satisfação\n- Feedback dos clientes\n\nFuncionalidade em desenvolvimento.";
        }
        catch (Exception ex)
        {
            _model.Mensagem = $"Erro: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }
}

public class SatisfacaoModel : INotifyPropertyChanged
{
    private string _mensagem = "Carregando análise de satisfação...";
    private bool _isLoading = false;

    public string Mensagem
    {
        get => _mensagem;
        set
        {
            _mensagem = value;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

