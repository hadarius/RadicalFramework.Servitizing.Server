using AutoMapper;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Radical.Servitizing;
using Radical.Servitizing.Data.Store;
using Radical.Servitizing.Server.Data.Service;

namespace Radical.Servitizing.Server
{
    public partial interface IServerSetup : IServiceSetup
    {      
        IServerSetup AddDataServer<TServiceStore>(DataServiceTypes dataServiceTypes, Action<DataServiceBuilder> builder) where TServiceStore : IDataServiceStore;
        IServerSetup ConfigureApiVersions(string[] apiVersions);
        IServerSetup ConfigureServer(bool includeSwagger, Assembly[] assemblies = null);
    }
}