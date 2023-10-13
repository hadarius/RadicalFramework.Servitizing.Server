using Radical.Series;
using Radical.Servitizing.Data.Store;
using System;

namespace Radical.Servitizing.Server.Data.Service
{
    public abstract class DataServiceBuilder : Registry<Type>, IDataServiceBuilder
    {
        protected virtual Type StoreType { get; set; }

        public static DataServiceTypes ServiceTypes { get; set; }

        public string RoutePrefix { get; set; } = "";

        public int PageLimit { get; set; } = 10000;

        protected ISeries<Type> ContextTypes { get; set; } = new Catalog<Type>();

        public DataServiceBuilder() : base()
        {
            RoutePrefix = GetRoutes();
        }

        public abstract void Build();

        protected virtual string GetRoutes()
        {
            if (StoreType == typeof(IEventStore))
            {
                return StoreRoutes.EventStore + RoutePrefix;
            }
            else
            {
                return StoreRoutes.DataStore + RoutePrefix;
            }
        }

        public virtual IDataServiceBuilder AddDataServices<TContext>() where TContext : DataStoreContext
        {
            TryAdd(typeof(TContext));
            return this;
        }
    }

    public enum DataServiceTypes
    {
        None = 0,
        Grpc = 1,
        OData = 2,
        Rest = 4,
        All = 7
    }
}
