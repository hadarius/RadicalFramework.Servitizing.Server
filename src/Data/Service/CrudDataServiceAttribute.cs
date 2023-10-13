using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Radical.Servitizing.Server.Data.Service
{
    public class CrudDataServiceAttribute : Attribute
    {
        public CrudDataServiceAttribute() { }
    }
}