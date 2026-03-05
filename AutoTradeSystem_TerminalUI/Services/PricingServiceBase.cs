using Microsoft.Extensions.Hosting; 

namespace AutoTradeSystem_TerminalUI.Services
{
    public abstract class PricingServiceBase : BackgroundService
    {
        protected PricingServiceBase()
        {
            
        }
        protected abstract Task UpdatePrices(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UpdatePrices(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
