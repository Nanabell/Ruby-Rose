using Discord.Commands;
using Discord.WebSocket;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RubyRose.Services
{
    public class ServiceHandler
    {
        private readonly List<ServiceBase> _registeredServices = new List<ServiceBase>();
        private readonly List<ServiceBase> _lateRegister = new List<ServiceBase>();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ServiceHandler(IDependencyMap map)
        {
            var client = map.Get<DiscordSocketClient>();
            ServiceBase.Client = client;
            ServiceBase.Map = map;

            var allInstantServices = Assembly.GetEntryAssembly().ExportedTypes
                .Where(x => x.GetTypeInfo().BaseType == typeof(ServiceBase));

            foreach (var s in allInstantServices)
            {
                var constructor = s.GetConstructors().FirstOrDefault(c => !c.GetParameters().Any());
                var service = (ServiceBase)constructor.Invoke(new object[0]);

                if (service.TryPreEnable().GetAwaiter().GetResult())
                {
                    _registeredServices.Add(service);
                    _logger.Info($"Registered and Enabled Service {s.Name}");
                }
                else
                {
                    _logger.Info($"Registered Service {s.Name}");
                    _registeredServices.Add(service);
                    _lateRegister.Add(service);
                }
            }
            if (_lateRegister.Count > 0)
                client.Ready += LateRegister;
        }

        private async Task LateRegister()
        {
            foreach (var service in _lateRegister)
            {
                await service.TryEnable();
            }
        }

        public TService GetService<TService>() where TService : ServiceBase
            => _registeredServices.FirstOrDefault(x => x.GetType() == typeof(TService)) as TService;

        public async Task<bool> TryEnable<TService>() where TService : ServiceBase
            => await (GetService<TService>()).TryEnable();

        public async Task<bool> TryDisable<TService>() where TService : ServiceBase
            => await (GetService<TService>()).TryDisable();
    }
}