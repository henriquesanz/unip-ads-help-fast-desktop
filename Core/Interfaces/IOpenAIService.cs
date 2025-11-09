using System.Threading;
using System.Threading.Tasks;

namespace HelpFastDesktop.Core.Interfaces;

public interface IOpenAIService
{
    Task<string> EnviarPerguntaAsync(string perguntaUsuario, string systemPrompt, CancellationToken cancellationToken = default);
}

