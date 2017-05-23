using Discord.Commands;
using Discord.WebSocket;
using NLog;
using System;
using System.Threading.Tasks;

namespace RubyRose.Services
{
    public abstract class ServiceBase
    {
        public static DiscordSocketClient Client;
        public static IServiceProvider Provider;
        internal static Logger Logger = LogManager.GetCurrentClassLogger();
        public bool IsEnabled { get; internal set; }

        protected abstract Task PreEnable();

        protected abstract Task PreDisable();

        protected abstract bool WaitForReady();

        public async Task<bool> TryPreEnable()
        {
            if (WaitForReady())
                return false;
            return await TryEnable();
        }

        public async Task<bool> TryEnable()
        {
            if (IsEnabled)
                return false;

            await PreEnable();
            IsEnabled = true;
            Logger.Info($"Enabled Service {GetType().Name}");
            return true;
        }

        public async Task<bool> TryDisable()
        {
            if (!IsEnabled)
                return false;

            await PreDisable();
            IsEnabled = false;
            Logger.Info($"Disabled Service {GetType().Name}");
            return true;
        }
    }
}