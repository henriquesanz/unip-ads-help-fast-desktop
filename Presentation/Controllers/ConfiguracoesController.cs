using HelpFastDesktop.Core.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class ConfiguracoesController : BaseController
{
    private ConfiguracoesModel _model;

    public ConfiguracoesController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _model = new ConfiguracoesModel();
    }

    public ConfiguracoesModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarConfiguracoesAsync()
    {
        try
        {
            _model.IsLoading = true;
            _model.Mensagem = "Configurações do Sistema:\n\n- Configurações gerais\n- Parâmetros do sistema\n- Integrações\n- Preferências\n\nFuncionalidade em desenvolvimento.";
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

public class ConfiguracoesModel : INotifyPropertyChanged
{
    private string _mensagem = "Carregando configurações...";
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

