using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using Service.Liquidity.ConverterMarkups.Jobs;

namespace Service.Liquidity.ConverterMarkups
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlClientLifeTime _myNoSqlClientLifeTime;
        private readonly AutoMarkupBackgroundJob _autoMarkupBackgroundJob;


        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger,
            MyNoSqlClientLifeTime myNoSqlClientLifeTime, 
            AutoMarkupBackgroundJob autoMarkupBackgroundJob)
            : base(appLifetime)
        {
            _logger = logger;
            _myNoSqlClientLifeTime = myNoSqlClientLifeTime;
            _autoMarkupBackgroundJob = autoMarkupBackgroundJob;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _myNoSqlClientLifeTime.Start();
            _autoMarkupBackgroundJob.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _autoMarkupBackgroundJob.Stop();
            _myNoSqlClientLifeTime.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
