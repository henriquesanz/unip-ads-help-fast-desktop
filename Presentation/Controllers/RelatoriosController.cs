using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
            _model.Mensagem = "Carregando dados do relatório...";

            var relatorio = await _relatorioService.GerarRelatorioSistemaAsync(_model.DataInicio, _model.DataFim);
            _model.AplicarRelatorio(relatorio);
            _model.Mensagem = $"Relatório gerado em {relatorio.GeradoEm:dd/MM/yyyy HH:mm}";
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

    public async System.Threading.Tasks.Task ExportarRelatorioAsync(RelatorioFormatoExportacao formato)
    {
        if (_model.RelatorioAtual == null)
        {
            _model.Mensagem = "Gere o relatório antes de exportar.";
            return;
        }

        try
        {
            var filtro = formato == RelatorioFormatoExportacao.Pdf
                ? "Arquivo PDF (*.pdf)|*.pdf"
                : "Arquivo Excel (*.xlsx)|*.xlsx";

            var extensao = formato == RelatorioFormatoExportacao.Pdf ? ".pdf" : ".xlsx";

            var dialog = new SaveFileDialog
            {
                Filter = filtro,
                FileName = $"relatorio_helpfast_{DateTime.Now:yyyyMMdd_HHmm}{extensao}"
            };

            var resultado = dialog.ShowDialog();
            if (resultado != true)
            {
                _model.Mensagem = "Exportação cancelada pelo usuário.";
                return;
            }

            var bytes = await _relatorioService.ExportarRelatorioSistemaAsync(_model.RelatorioAtual, formato);
            await File.WriteAllBytesAsync(dialog.FileName, bytes);

            _model.Mensagem = $"Relatório exportado com sucesso para {dialog.FileName}";
        }
        catch (Exception ex)
        {
            _model.Mensagem = $"Erro ao exportar relatório: {ex.Message}";
        }
    }
}

public class RelatoriosModel : INotifyPropertyChanged
{
    private string _mensagem = "Carregando relatórios...";
    private bool _isLoading = false;
    private DateTime? _dataInicio;
    private DateTime? _dataFim;
    private RelatorioSistemaResumoDto? _resumo;
    private RelatorioSistemaDto? _relatorioAtual;

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
            OnPropertyChanged(nameof(PodeExportar));
        }
    }

    public DateTime? DataInicio
    {
        get => _dataInicio;
        set
        {
            _dataInicio = value;
            OnPropertyChanged();
        }
    }

    public DateTime? DataFim
    {
        get => _dataFim;
        set
        {
            _dataFim = value;
            OnPropertyChanged();
        }
    }

    public RelatorioSistemaResumoDto? Resumo
    {
        get => _resumo;
        private set
        {
            _resumo = value;
            OnPropertyChanged();
        }
    }

    public RelatorioSistemaDto? RelatorioAtual
    {
        get => _relatorioAtual;
        private set
        {
            _relatorioAtual = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PodeExportar));
            OnPropertyChanged(nameof(PossuiDados));
        }
    }

    public bool PodeExportar => !_isLoading && _relatorioAtual != null;

    public bool PossuiDados => _relatorioAtual != null;

    public ObservableCollection<CargoRelatorioDto> Cargos { get; } = new();
    public ObservableCollection<UsuarioRelatorioDto> Usuarios { get; } = new();
    public ObservableCollection<ChamadoRelatorioDto> Chamados { get; } = new();
    public ObservableCollection<ChatRelatorioDto> Chats { get; } = new();
    public ObservableCollection<ChatIaRelatorioDto> ChatIaResults { get; } = new();
    public ObservableCollection<HistoricoChamadoRelatorioDto> HistoricosChamados { get; } = new();
    public ObservableCollection<FaqRelatorioDto> Faqs { get; } = new();

    public RelatoriosModel()
    {
        DataInicio = DateTime.Today.AddDays(-30);
        DataFim = DateTime.Today;
    }

    public void AplicarRelatorio(RelatorioSistemaDto relatorio)
    {
        AtualizarColecao(Cargos, relatorio.Cargos);
        AtualizarColecao(Usuarios, relatorio.Usuarios);
        AtualizarColecao(Chamados, relatorio.Chamados);
        AtualizarColecao(Chats, relatorio.Chats);
        AtualizarColecao(ChatIaResults, relatorio.ChatIaResults);
        AtualizarColecao(HistoricosChamados, relatorio.HistoricosChamados);
        AtualizarColecao(Faqs, relatorio.Faqs);

        Resumo = relatorio.Resumo;
        RelatorioAtual = relatorio;
    }

    private static void AtualizarColecao<T>(ObservableCollection<T> destino, IEnumerable<T> origem)
    {
        destino.Clear();
        foreach (var item in origem)
        {
            destino.Add(item);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

