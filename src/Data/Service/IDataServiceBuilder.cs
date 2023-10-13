using Radical.Servitizing.Data.Store;
using System;

namespace Radical.Servitizing.Server.Data.Service;
public interface IDataServiceBuilder<TStore> : IDataServiceBuilder where TStore : IDataServiceStore { }

public interface IDataServiceBuilder : IDisposable, IAsyncDisposable
{
    string RoutePrefix { get; set; }

    int PageLimit { get; set; }

    void Build();

    IDataServiceBuilder AddDataServices<TContext>() where TContext : DataStoreContext;
}
