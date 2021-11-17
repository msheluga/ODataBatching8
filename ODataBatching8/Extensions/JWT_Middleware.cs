using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using ODataBatching8.Service;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODataBatching8.Extensions
{
    public class JWT_Middleware
    {
        private readonly RequestDelegate _request;

        public JWT_Middleware(RequestDelegate request)
        {
            _request = request;

        }


        public async Task Invoke(HttpContext context)
        {

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
                attachUserToContext(context, token);

            await _request(context);
        }

        private async void attachUserToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("my_secret_key_12345");

                var securityKey = new SymmetricSecurityKey(key);
                var configString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["BookDatabase"];
                var userEdm = BooksContextService.GetEdmModel(configString);
                //context.Request.ODataFeature().Model = userEdm;

                //var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                //tokenHandler.ValidateToken(token, new TokenValidationParameters
                //{
                //    ValidateIssuerSigningKey = true,
                //    IssuerSigningKey = new SymmetricSecurityKey(key),
                //    ValidateIssuer = false,
                //    ValidateAudience = false,
                //    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                //    //ClockSkew = TimeSpan.Zero
                //}, out SecurityToken validatedToken);

                //var jwtToken = (JwtSecurityToken)validatedToken;
                //var userId = jwtToken.Claims.First(x => x.Type.Equals("UserKey")).Value;

                // attach user to context on successful jwt validation
                //var query = new GetUserByEDIPIQuery(userId);
                //var userData = await mediator.Send(query);
                //context.Items["User"] = userData;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
