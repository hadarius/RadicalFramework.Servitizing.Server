using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Radical.Servitizing.Server.API.Documentation
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class IgnoreApiAttribute : ActionFilterAttribute { }
}

