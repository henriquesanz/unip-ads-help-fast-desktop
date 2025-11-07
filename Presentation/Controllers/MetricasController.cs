using HelpFastDesktop.Core.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class MetricasController : BaseController
{
    private MetricasModel _model;

    public MetricasController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _model = new MetricasModel();
    }

    public MetricasModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarMetricasAsync()
    {
        try
        {
            _model.IsLoading = true;
            _model.Mensagem = "Métricas de Performance:\n\n- Tempo médio de resolução\n- Taxa de satisfação\n- Chamados por técnico\n- Performance por período\n\nFuncionalidade em desenvolvimento.";
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

public class MetricasModel : INotifyPropertyChanged
{
    private string _mensagem = "Carregando métricas...";
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

