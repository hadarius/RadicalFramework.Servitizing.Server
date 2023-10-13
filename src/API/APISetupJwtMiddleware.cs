
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Radical.Servitizing.Server.API;

public class APISetupJwtMiddleware
{
    private readonly RequestDelegate _next;

    public APISetupJwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var ao = ServiceManager.GetConfiguration().Identity;
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var client = new HttpClient();

        var response = await client.GetUserInfoAsync(new UserInfoRequest
        {
            Address = $"{ao.BaseUrl}/connect/userinfo",
            Token = token
        });
        //response.

        //if (userId != null)
        //{
        //    // attach user to context on successful jwt validation
        //    context.Items["User"] = await userRepository.GetUser(userId.Value);
        //}

        await _next(context);
    }
}