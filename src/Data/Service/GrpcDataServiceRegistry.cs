using Radical.Series;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Radical.Servitizing.Server.Data.Service
{
    public static class GrpcDataServiceRegistry
    {
        public static ISeries<Type> ServiceContracts = new Registry<Type>();
    }
}
