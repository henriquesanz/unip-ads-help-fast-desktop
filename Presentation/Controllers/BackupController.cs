using HelpFastDesktop.Core.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HelpFastDesktop.Presentation.Controllers;

public class BackupController : BaseController
{
    private BackupModel _model;

    public BackupController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _model = new BackupModel();
    }

    public BackupModel GetModel() => _model;

    public async System.Threading.Tasks.Task CarregarInfoBackupAsync()
    {
        try
        {
            _model.IsLoading = true;
            _model.Mensagem = "Gerenciamento de Backup:\n\n- Backup automático configurado\n- Último backup: Em desenvolvimento\n- Restauração de backup\n- Agendamento de backups\n\nFuncionalidade em desenvolvimento.";
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

public class BackupModel : INotifyPropertyChanged
{
    private string _mensagem = "Carregando informações de backup...";
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

