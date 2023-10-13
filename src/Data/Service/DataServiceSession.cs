
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.Data.Service
{
    public class DataServiceSession
    {
        private readonly RequestDelegate _next;

        public DataServiceSession(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
        }
    }
}