using Microsoft.Extensions.DependencyInjection;

namespace HelpFastDesktop.Presentation.Controllers;

public abstract class BaseController
{
    protected IServiceProvider ServiceProvider { get; }

    protected BaseController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}
