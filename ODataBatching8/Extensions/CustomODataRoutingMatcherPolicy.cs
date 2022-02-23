using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataBatching8.Models;
using ODataBatching8.Service;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODataBatching8.Extensions
{
    public class CustomODataRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private IODataTemplateTranslator _translator;
        private IDbContextFactory<BooksContext> _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutingMatcherPolicy" /> class.
        /// </summary>
        /// <param name="translator">The registered path template translator.</param>
        public CustomODataRoutingMatcherPolicy(IODataTemplateTranslator translator, IDbContextFactory<BooksContext> context)
        {
            _translator = translator;
            _context = context;
            
        }

        /// <summary>
        /// Gets a value that determines the order of this policy.
        /// </summary>
        public override int Order => 900;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return endpoints.Any(e => e.Metadata.OfType<ODataRoutingMetadata>().FirstOrDefault() != null);
        }

        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            IODataFeature odataFeature = httpContext.ODataFeature();
            Guid user = default;
            var configString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["BookDatabase"];
            string auth = httpContext.Request.Headers["Authorization"];

            if (odataFeature.Path != null)
            {
                // If we have the OData path setting, it means there's some Policy working.
                // Let's skip this default OData matcher policy.
                return Task.CompletedTask;
            }

            var isValid = validateAuthHeader(auth, out user);

            if (user.ToString().Equals(new Guid().ToString()) || !isValid)
            {
                return Task.FromException(new Exception("Forbidden"));
            }
                // The goal of this method is to perform the final matching:
                // Map between route values matched by the template and the ones we want to expose to the action for binding.
                // (tweaking the route values is fine here)
                // Invalidating the candidate if the key/function values are not valid/missing.
                // Perform overload resolution for functions by looking at the candidates and their metadata.
                for (var i = 0; i < candidates.Count; i++)
            {
               
                IODataRoutingMetadata metadata = null;                

                ref CandidateState candidate = ref candidates[i];
                
                if (!candidates.IsValidCandidate(i))
                {
                    continue;
                }                                
                                
                    
                IEdmModel model = BooksContextService.GetEdmModel(configString, user.ToString());
                httpContext.Request.ODataFeature().Model = model;
                if (candidate.Values.Any(v => v.Key.Equals("controller") && v.Value.Equals("Metadata")))
                {
                    metadata = candidate.Endpoint.Metadata.OfType<IODataRoutingMetadata>().FirstOrDefault();
                    SetODataPath(httpContext, candidates, odataFeature, i, metadata, candidate);
                    return Task.CompletedTask;
                }
                else
                {
                    IEdmEntitySet entitySet = model.FindDeclaredEntitySet(candidate.Values["controller"].ToString());
                    var entityType = Type.GetType("ODataBatching8.Models." + candidate.Values["controller"].ToString().Singularize());
                    if (entitySet == null)
                    {
                        return Task.FromException(new Exception("Forbidden"));                            
                    }
                    var entityDeclaredType = model.FindDeclaredType(entityType.FullName);
                    var edmEntityType = entityDeclaredType as IEdmEntityType;
                    IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>
                    {
                        new EntitySetSegmentTemplate(entitySet),
                        CreateKeySegment(edmEntityType, entitySet)
                    };
                    var pathTemplate = new ODataPathTemplate(segments);
                    metadata = new ODataRoutingMetadata("odata", model, pathTemplate);
                    if (metadata == null)
                    {
                        continue;
                    }
                    if (odataFeature.Path != null)
                    {
                        // If it's odata endpoint, and we have a path set, let other odata endpoints invalid.
                        candidates.SetValidity(i, false);
                        continue;
                    }
                    SetODataPath(httpContext, candidates, odataFeature, i, metadata, candidate);
                }
                

                if (metadata == null)
                {
                    continue;
                }
            }
            return Task.CompletedTask; 
        }

        private void SetODataPath(HttpContext httpContext, CandidateSet candidates, IODataFeature odataFeature, int i, IODataRoutingMetadata metadata, CandidateState candidate)
        {
            ODataTemplateTranslateContext translatorContext =
                                    new ODataTemplateTranslateContext(httpContext, candidate.Endpoint, candidate.Values, metadata.Model);
            try
            {
                ODataPath odataPath = _translator.Translate(metadata.Template, translatorContext);
                if (odataPath != null)
                {
                    odataFeature.RoutePrefix = metadata.Prefix;
                    odataFeature.Model = metadata.Model;
                    odataFeature.Path = odataPath;
                    MergeRouteValues(translatorContext.UpdatedValues, candidate.Values);
                    // Shall we break the remaining candidates?
                    // So far the answer is no. Because we can use this matcher to obsolete the unmatched endpoint.
                    // break;
                }
                else
                {
                    candidates.SetValidity(i, false);
                }
            }
            catch
            {
            }
        }

        private bool validateAuthHeader(string auth, out Guid user)
        {
            var valid = false;
            user = new Guid();
            if (String.IsNullOrEmpty(auth))
            {
                return valid;
            }
            try
            {
                var token = auth.Split(" ").Last();
                var tokenHandler = new JwtSecurityTokenHandler();
                var rawSecret = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["Secret"];
                var key = Encoding.ASCII.GetBytes(rawSecret);
                var securityKey = new SymmetricSecurityKey(key);
                
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    //ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type.Equals("userid")).Value;
                user = new Guid(userId);
                valid = true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
            return valid;
        }

        private static void MergeRouteValues(RouteValueDictionary updatedValues, RouteValueDictionary values)
        {
            foreach (var data in updatedValues)
            {
                values[data.Key] = data.Value;
            }
        }

        private static KeySegmentTemplate CreateKeySegment(IEdmEntityType edmEntityType, IEdmNavigationSource navigationSource, string keyPrefix = "key")
        {
            if (edmEntityType == null)
            {
                throw new ArgumentNullException(nameof(edmEntityType));
            }
            IDictionary<string, string> keyTemplates = new Dictionary<string, string>();
            var keys = edmEntityType.Key().ToArray();
            if (keys.Length == 1)
            {
                keyTemplates[keys[0].Name] = $"{{{keyPrefix}}}";
            }
            else
            {
                foreach (var key in keys)
                {
                    keyTemplates[key.Name] = $"{{{keyPrefix}{key.Name}}}";
                }
            }
            return new KeySegmentTemplate(keyTemplates, edmEntityType, navigationSource);
        }
    }
}
