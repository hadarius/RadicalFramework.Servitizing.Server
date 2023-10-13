using ProtoBuf.Grpc.Configuration;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Radical.Servitizing.Server
{
    internal class ServerGrpcServiceBinder : ServiceBinder
    {
        private readonly IServiceRegistry registry;

        public ServerGrpcServiceBinder(IServiceRegistry registry)
        {
            this.registry = registry;
        }

        public override IList<object> GetMetadata(MethodInfo method, Type contractType, Type serviceType)
        {
            var resolvedServiceType = serviceType;
            if (serviceType.IsInterface)
                resolvedServiceType = registry[serviceType]?.ImplementationType ?? serviceType;

            return base.GetMetadata(method, contractType, resolvedServiceType);
        }
    }
}