using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataBatching8.Models;
using ODataBatching8.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataBatching8
{
    public class CustomODataRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private IODataTemplateTranslator _translator;
        private IDbContextFactory<BooksContext> _context;
        private IDistributedCache _cache;
        private IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutingMatcherPolicy" /> class.
        /// </summary>
        /// <param name="translator">The registered path template translator.</param>
        public CustomODataRoutingMatcherPolicy(IODataTemplateTranslator translator,
                                                IDbContextFactory<BooksContext> context,
                                                IDistributedCache cache,
                                                IConfiguration config)
        {
            _translator = translator;
            _context = context;
            _cache = cache;
            _config = config;
        }

        /// <summary>
        /// Gets a value that determines the order of this policy.
        /// </summary>
        public override int Order => 899;

        /// <summary>
        /// Returns a value that indicates whether the matcher applies to any endpoint in endpoints.
        /// </summary>
        /// <param name="endpoints">The set of candidate values.</param>
        /// <returns>true if the policy applies to any endpoint in endpoints, otherwise false.</returns>
        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return endpoints.Any(e => e.Metadata.OfType<ODataRoutingMetadata>().FirstOrDefault() != null);
        }

        /// <summary>
        /// Applies the policy to the CandidateSet.
        /// </summary>
        /// <param name="httpContext">The context associated with the current request.</param>
        /// <param name="candidates">The CandidateSet.</param>
        /// <returns>The task.</returns>
        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            IODataFeature odataFeature = httpContext.ODataFeature();
            if (odataFeature.Path != null)
            {
                // If we have the OData path setting, it means there's some Policy working.
                // Let's skip this default OData matcher policy.
                return Task.CompletedTask;
            }

            // The goal of this method is to perform the final matching:
            // Map between route values matched by the template and the ones we want to expose to the action for binding.
            // (tweaking the route values is fine here)
            // Invalidating the candidate if the key/function values are not valid/missing.
            // Perform overload resolution for functions by looking at the candidates and their metadata.
            for (var i = 0; i < candidates.Count; i++)
            {
                ref CandidateState candidate = ref candidates[i];
                if (!candidates.IsValidCandidate(i))
                {
                    continue;
                }

                IODataRoutingMetadata metadata = null;

                Guid user = default;
                string auth = httpContext.Request.Headers["Authorization"];
                if (auth != null)
                {
                    user = Guid.Parse(auth["bearer ".Length..].Trim());
                }
                IEdmModel model = BooksContextService.GetEdmModel(_config.GetConnectionString("BookDatabase"));
                httpContext.Request.ODataFeature().Model = model;

                if (candidate.Values.Any(v => v.Key.Equals("controller") && v.Value.Equals("Metadata")))
                {
                    metadata = candidate.Endpoint.Metadata.OfType<IODataRoutingMetadata>().FirstOrDefault();
                }
                else
                {
                    if (HttpMethods.IsDelete(httpContext.Request.Method))
                    {
                        IEdmModel godModel = httpContext.Request.ODataOptions().RouteComponents["odata"].EdmModel;
                        var controllerName = candidate.Values["controller"].ToString();
                        var godType = godModel.SchemaElements.FirstOrDefault(s => s.Name.Equals(controllerName)) as IEdmStructuredType;
                        var dynamicType = model.SchemaElements.FirstOrDefault(s => s.Name.Equals(controllerName)) as IEdmStructuredType;
                        if (godType.DeclaredProperties.Count() != dynamicType.DeclaredProperties.Count())
                        {
                            throw new Exception("Unauthorized");
                        }
                    }

                    httpContext.Request.ODataFeature().Model = model;
                    IEdmEntitySet entitySet = model.FindDeclaredEntitySet(candidate.Values["controller"].ToString());

                    var type = Type.GetType("TestApi2.Models." + candidate.Values["controller"].ToString());
                    var entityType = model.FindDeclaredType(type.FullName) as IEdmEntityType;

                    IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>
                    {
                        new EntitySetSegmentTemplate(entitySet),
                        CreateKeySegment(entityType, entitySet)
                    };

                    metadata = new ODataRoutingMetadata("odata", model, new ODataPathTemplate(segments));
                }

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

                ODataTemplateTranslateContext translatorContext =
                    new ODataTemplateTranslateContext(httpContext, candidate.Endpoint, candidate.Values, metadata.Model);

                ODataPath odataPath = _translator.Translate(metadata.Template, translatorContext);
                if (odataPath != null)
                {
                    odataFeature.RoutePrefix = metadata.Prefix;
                    //odataFeature.Model = metadata.Model;
                    odataFeature.Model = model;
                    odataFeature.Path = odataPath;

                    MergeRouteValues(translatorContext.UpdatedValues, candidate.Values);
                }
                else
                {
                    candidates.SetValidity(i, false);
                }
            }

            return Task.CompletedTask;
        }

        private static void MergeRouteValues(RouteValueDictionary updates, RouteValueDictionary source)
        {
            foreach (var data in updates)
            {
                source[data.Key] = data.Value;
            }
        }

        internal static KeySegmentTemplate CreateKeySegment(IEdmEntityType entityType, IEdmNavigationSource navigationSource, string keyPrefix = "key")
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            IDictionary<string, string> keyTemplates = new Dictionary<string, string>();
            var keys = entityType.Key().ToArray();
            if (keys.Length == 1)
            {
                // Id={key}
                keyTemplates[keys[0].Name] = $"{{{keyPrefix}}}";
            }
            else
            {
                // Id1={keyId1},Id2={keyId2}
                foreach (var key in keys)
                {
                    keyTemplates[key.Name] = $"{{{keyPrefix}{key.Name}}}";
                }
            }

            return new KeySegmentTemplate(keyTemplates, entityType, navigationSource);
        }
    }
}
