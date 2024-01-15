using System.ComponentModel.Design;
using System.Diagnostics;

namespace hoa7mlishe.Interfaces.Server
{
    public sealed class ServerServices
    {
        private static IServiceContainer _services = null;

        public static void SetServiceContainer(IServiceContainer services) => _services ??= services;

        public static IServiceContainer ServiceContainer
        {
            [DebuggerStepThrough]
            get
            {
                return _services;
            }
        }

        public static void RemoveService(Type serviceType)
        {
            _services.RemoveService(serviceType);
        }

        public static void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            _services.AddService(serviceType, callback);
        }

        public static void AddService(Type serviceType, object serviceInstance)
        {
            _services.AddService(serviceType, serviceInstance);
        }

        public static object GetService(Type serviceType) => _services.GetService(serviceType);
    }
}
