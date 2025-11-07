using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class RelatoriosController : BaseController
{
    private readonly IRelatorioService _relatorioService;
    private RelatoriosModel _model;

    public RelatoriosController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _relatorioService = serviceProvider.GetRequiredService<IRelatorioService>();
        _model = new RelatoriosModel();
    }

    public RelatoriosModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarRelatoriosAsync()
    {
        try
        {
            _model.IsLoading = true;
            _model.Mensagem = "Relatórios disponíveis:\n\n- Relatório de Chamados\n- Relatório de Performance\n- Relatório de Satisfação\n\nFuncionalidade em desenvolvimento.";
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

public class RelatoriosModel : INotifyPropertyChanged
{
    private string _mensagem = "Carregando relatórios...";
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

