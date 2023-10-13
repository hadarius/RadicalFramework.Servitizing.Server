using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Radical.Servitizing.Server.Data.Service;

using API.Controller;
using DTO;
using Servitizing.Data.Store;

public class RestDataServiceBuilder<TStore> : DataServiceBuilder, IDataServiceBuilder<TStore> where TStore : IDataServiceStore
{
    IServiceRegistry _registry;

    public RestDataServiceBuilder() : base()
    {
        _registry = ServiceManager.GetManager().Registry;
        StoreType = typeof(TStore);
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
                            type => type.GetCustomAttribute<CrudDataServiceAttribute>()
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
                ifaceType = typeof(ICrudDataController<,,>).MakeGenericType(new[] { genTypes[0], genTypes[3], genTypes[4] });
            else if (genTypes.Length > 3)
                if (genTypes[3].IsAssignableTo(typeof(IDTO)) && storeTypes.Contains(genTypes[1]))
                    ifaceType = typeof(ICrudDataController<,,>).MakeGenericType(new[] { genTypes[0], genTypes[2], genTypes[3] });
                else
                    continue;

            _registry.AddScoped(ifaceType, controllerType);
        }
    }

    public override void Build()
    {
        //BuildModel();
    }

    protected override string GetRoutes()
    {
        if (StoreType == typeof(IEventStore))
        {
            return StoreRoutes.CrudEventStore;
        }
        else
        {
            return StoreRoutes.CrudDataStore;
        }
    }
}