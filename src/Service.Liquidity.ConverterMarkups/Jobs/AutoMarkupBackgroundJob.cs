using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;

namespace Service.Liquidity.ConverterMarkups.Jobs
{
    public class AutoMarkupBackgroundJob
    {
        private readonly ILogger<AutoMarkupBackgroundJob> _logger;
        private readonly MyTaskTimer _operationsTimer;
        private const int TimerSpanSec = 30;

        public AutoMarkupBackgroundJob(ILogger<AutoMarkupBackgroundJob> logger)
        {
            _logger = logger;
            _operationsTimer = new MyTaskTimer(nameof(AutoMarkupBackgroundJob),
                TimeSpan.FromSeconds(TimerSpanSec), logger, Process);
        }

        public void Start()
        {
            _operationsTimer.Start();
        }

        public void Stop()
        {
            _operationsTimer.Stop();
        }

        private async Task Process()
        {
            await Task.Delay(TimerSpanSec);
        }
    }
}
