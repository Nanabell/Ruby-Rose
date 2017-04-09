using Discord.Commands;
using Discord.WebSocket;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Services
{
    public abstract class ServiceBase
    {
        public static DiscordSocketClient Client;
        public static IDependencyMap Map;
        internal static Logger _logger = LogManager.GetCurrentClassLogger();
        public bool IsEnabled { get; internal set; }

        protected abstract Task PreEnable();

        protected abstract Task PreDisable();

        protected abstract bool WaitForReady();

        public async Task<bool> TryPreEnable()
        {
            if (WaitForReady())
                return false;
            else
                return await TryEnable();
        }

        public async Task<bool> TryEnable()
        {
            if (IsEnabled)
                return false;

            await PreEnable();
            IsEnabled = true;
            _logger.Info($"Enabled Service {GetType().Name}");
            return true;
        }

        public async Task<bool> TryDisable()
        {
            if (!IsEnabled)
                return false;

            await PreDisable();
            IsEnabled = false;
            _logger.Info($"Disabled Service {GetType().Name}");
            return true;
        }
    }
}