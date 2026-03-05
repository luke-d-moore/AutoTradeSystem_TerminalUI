using AutoTradeSystem_TerminalUI.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PricingSystem.Protos;
using System.Collections.Concurrent;

namespace AutoTradeSystem_TerminalUI.Services
{
    public class PricingService : PricingServiceBase, IPricingService
    {
        private readonly GrpcPricingService.GrpcPricingServiceClient _grpcClient;
        private ConcurrentDictionary<string, decimal> _prices = new ConcurrentDictionary<string, decimal>();
        private readonly TaskCompletionSource<bool> _initialpriceLoad = new();
        private int _retryInterval = 5000;
        public Task InitialPriceLoadTask => _initialpriceLoad.Task;
        public ConcurrentDictionary<string, decimal> Prices => _prices;
        public PricingService(GrpcPricingService.GrpcPricingServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
        }
        protected override async Task UpdatePrices(CancellationToken cancellationToken)
        {
            using var call = _grpcClient.GetLatestPrices(new Empty());

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var priceUpdate in call.ResponseStream.ReadAllAsync())
                    {
                        Prices[priceUpdate.Symbol] = (decimal)priceUpdate.Price;
                    }

                    await Task.Delay(_retryInterval);
                }
                catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (RpcException)
                {
                    await Task.Delay(_retryInterval);
                    break;
                }
                catch (Exception)
                {
                }
            }
        }

        public IDictionary<string, decimal> GetLatestPrices()
        {
            return Prices;
        }
        public IEnumerable<string> GetLatestTickers()
        {
            return Prices.Keys;
        }
    }
}
