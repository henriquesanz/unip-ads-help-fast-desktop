using HelpFastDesktop.Core.Models.Entities;
using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class AuditoriaController : BaseController
{
    private readonly IAuditoriaService _auditoriaService;
    private AuditoriaModel _model;

    public AuditoriaController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _auditoriaService = serviceProvider.GetRequiredService<IAuditoriaService>();
        _model = new AuditoriaModel();
    }

    public AuditoriaModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarLogsAsync()
    {
        try
        {
            _model.IsLoading = true;
            var dataInicio = DateTime.Now.AddDays(-30);
            var dataFim = DateTime.Now;
            
            var logs = await _auditoriaService.ObterLogsAsync(dataInicio, dataFim);
            
            _model.Logs.Clear();
            foreach (var log in logs.OrderByDescending(l => l.DataAcao))
            {
                _model.Logs.Add(log);
            }
        }
        catch (Exception ex)
        {
            _model.ErrorMessage = $"Erro ao carregar logs: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }
}

public class AuditoriaModel : INotifyPropertyChanged
{
    private ObservableCollection<LogAuditoria> _logs = new();
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public ObservableCollection<LogAuditoria> Logs
    {
        get => _logs;
        set
        {
            _logs = value;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

