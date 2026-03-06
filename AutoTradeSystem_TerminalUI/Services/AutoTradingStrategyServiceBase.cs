using Microsoft.Extensions.Hosting; 

namespace AutoTradeSystem_TerminalUI.Services
{
    public abstract class AutoTradingStrategyServiceBase : BackgroundService
    {
        protected AutoTradingStrategyServiceBase()
        {
            
        }
        protected abstract Task UpdateStrategies(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UpdateStrategies(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
