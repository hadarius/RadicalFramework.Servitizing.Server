using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Grpc.Server;
using Radical.Servitizing.Data.Store;
using System;
using System.Linq;
using System.Reflection;

namespace Radical.Servitizing.Server.Data.Service;

using DTO;
using API.Controller;

public class GrpcDataServiceBuilder<TServiceStore> : DataServiceBuilder, IDataServiceBuilder<TServiceStore> where TServiceStore : IDataServiceStore
{
    IServiceRegistry _registry;

    public GrpcDataServiceBuilder() : base()
    {
        _registry = ServiceManager.GetRegistry();
        StoreType = typeof(TServiceStore);
    }

    public void BuildModel()
    {
        Type[] storeTypes = DataStoreRegistry.Stores.Where(s => s.IsAssignableTo(StoreType)).ToArray();

        if (!storeTypes.Any())
            return;

        Assembly[] asm = AppDomain.CurrentDomain.GetAssemblies();
        var controllerTypes = asm.SelectMany(
                a =>
                    a.GetTypes()
                        .Where(
                            type => type.GetCustomAttribute<StreamDataServiceAttribute>()
                                    != null
                        )
                        .ToArray())
            .Where(
                b =>
                    !b.IsAbstract
                    && b.BaseType.IsGenericType
                    && b.BaseType.GenericTypeArguments.Length > 3
            ).ToArray();

        foreach (var controllerType in controllerTypes)
        {
            Type ifaceType = null;
            var genTypes = controllerType.BaseType.GenericTypeArguments;

            if (genTypes.Length > 4 && storeTypes.Contains(genTypes[1]) || storeTypes.Contains(genTypes[2]))
                ifaceType = typeof(IStreamDataController<>).MakeGenericType(new[] { genTypes[4] });
            else if (genTypes.Length > 3)
                if (genTypes[3].IsAssignableTo(typeof(IDTO)) && storeTypes.Contains(genTypes[1]))
                    ifaceType = typeof(IStreamDataController<>).MakeGenericType(new[] { genTypes[3] });
                else
                    continue;

            GrpcDataServiceRegistry.ServiceContracts.Add(ifaceType);

            _registry.AddSingleton(ifaceType, controllerType.New());
        }
    }

    public void Configure()
    {
        throw new NotImplementedException();
    }

    public override void Build()
    {
        BuildModel();
    }

    protected override string GetRoutes()
    {
        if (StoreType == typeof(IEventStore))
        {
            return StoreRoutes.StreamEventStore;
        }
        else
        {
            return StoreRoutes.StreamDataStore;
        }
    }

    public virtual void AddGrpcServicer()
    {
        _registry.AddCodeFirstGrpc(config =>
        {
            config.ResponseCompressionLevel = System
                .IO
                .Compression
                .CompressionLevel
                .NoCompression;
        });
        _registry.TryAddSingleton(
            BinderConfiguration.Create(binder: new ServerGrpcServiceBinder(_registry))
        );
        _registry.AddCodeFirstGrpcReflection();
    }
}