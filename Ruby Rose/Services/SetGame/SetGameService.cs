using System.Threading.Tasks;

namespace RubyRose.Services.SetGame
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
            Logger.Info($"Set Game to: {_config.Game}");
            await Client.SetGameAsync(_config.Game);
        }
    }
}