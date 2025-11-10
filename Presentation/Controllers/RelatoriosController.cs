using HelpFastDesktop.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
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

    public void Inicializar()
    {
        _model.PopularMeses();
        _model.Mensagem = "Selecione o mês e clique em baixar relatório.";
    }

    public async System.Threading.Tasks.Task ExportarRelatorioMensalAsync()
    {
        if (_model.MesSelecionado == null)
        {
            _model.Mensagem = "Escolha um mês antes de exportar.";
            return;
        }

        try
        {
            _model.IsLoading = true;
            _model.Mensagem = "Gerando relatório...";

            var periodoInicio = new DateTime(_model.MesSelecionado.Ano, _model.MesSelecionado.Mes, 1);
            var periodoFim = periodoInicio.AddMonths(1).AddTicks(-1);

            var relatorio = await _relatorioService.GerarRelatorioSistemaAsync(periodoInicio, periodoFim);
            var bytes = await _relatorioService.ExportarRelatorioSistemaAsync(relatorio, RelatorioFormatoExportacao.Pdf);

            var dialog = new SaveFileDialog
            {
                Filter = "Arquivo PDF (*.pdf)|*.pdf",
                FileName = $"relatorio_helpfast_{periodoInicio:yyyy_MM}.pdf"
            };

            var resultado = dialog.ShowDialog();
            if (resultado != true)
            {
                _model.Mensagem = "Download cancelado pelo usuário.";
                return;
            }

            await File.WriteAllBytesAsync(dialog.FileName, bytes);

            _model.Mensagem = $"Relatório baixado com sucesso em {dialog.FileName}";
        }
        catch (Exception ex)
        {
            _model.Mensagem = $"Erro ao gerar relatório: {ex.Message}";
        }
        finally
        {
            _model.IsLoading = false;
        }
    }
}

public class RelatoriosModel : INotifyPropertyChanged
{
    private string _mensagem = string.Empty;
    private bool _isLoading;
    private ObservableCollection<RelatorioMesOpcao> _mesesDisponiveis = new();
    private RelatorioMesOpcao? _mesSelecionado;

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

    public ObservableCollection<RelatorioMesOpcao> MesesDisponiveis
    {
        get => _mesesDisponiveis;
        set
        {
            _mesesDisponiveis = value;
            OnPropertyChanged();
        }
    }

    public RelatorioMesOpcao? MesSelecionado
    {
        get => _mesSelecionado;
        set
        {
            _mesSelecionado = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PodeExportar));
        }
    }

    public bool PodeExportar => !_isLoading && _mesSelecionado != null;

    public void PopularMeses(int quantidadeMeses = 12)
    {
        MesesDisponiveis.Clear();
        var referencia = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        for (int i = 0; i < quantidadeMeses; i++)
        {
            var data = referencia.AddMonths(-i);
            MesesDisponiveis.Add(new RelatorioMesOpcao(data));
        }

        MesSelecionado = MesesDisponiveis.FirstOrDefault();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public record RelatorioMesOpcao
{
    public int Ano { get; }
    public int Mes { get; }
    public string Descricao { get; }

    public RelatorioMesOpcao(DateTime data)
    {
        Ano = data.Year;
        Mes = data.Month;
        Descricao = data.ToString("MMMM \\de yyyy", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")).ToUpperInvariant();
    }
}

