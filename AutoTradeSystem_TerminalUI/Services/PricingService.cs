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
        private int _retryInterval = 5000;
        public ConcurrentDictionary<string, decimal> Prices => _prices;
        public PricingService(GrpcPricingService.GrpcPricingServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
        }
        protected override async Task UpdatePrices(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var call = _grpcClient.GetLatestPrices(new Empty());

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
                catch (RpcException ex)
                {
                    await Task.Delay(_retryInterval);
                }
                catch (Exception ex)
                {
                    await Task.Delay(_retryInterval);
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
