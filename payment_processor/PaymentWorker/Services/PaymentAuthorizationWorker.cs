namespace PaymentWorker.Services
{
    public sealed class PaymentAuthorizationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentAuthorizationWorker> _logger;

        public PaymentAuthorizationWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentAuthorizationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment authorization worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var processor = scope.ServiceProvider
                        .GetRequiredService<PaymentAuthorizationProcessor>();

                    await processor.ProcessPendingAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in payment authorization worker loop.");
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }

            _logger.LogInformation("Payment authorization worker stopped.");
        }
    }
}
