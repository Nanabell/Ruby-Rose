using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Services
{
    public class SetGameService : ServiceBase
    {
        private CoreConfig _config;

        protected override Task PreDisable()
        {
            return Task.CompletedTask;
        }

        protected override async Task PreEnable()
        {
            await SetGame();
        }

        protected override bool WaitForReady()
            => true;

        private async Task SetGame()
        {
            _config = Map.Get<CoreConfig>();
            _logger.Info($"Set Game to: {_config.Game}");
            await Client.SetGameAsync(_config.Game);
        }
    }
}