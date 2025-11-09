using System.Threading;
using System.Threading.Tasks;

namespace HelpFastDesktop.Core.Interfaces;

public interface IGoogleDriveService
{
    Task<string> LerDocumentoComoStringAsync(string fileId, CancellationToken cancellationToken = default);
}

