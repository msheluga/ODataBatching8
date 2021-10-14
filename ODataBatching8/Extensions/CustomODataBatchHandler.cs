using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq;
using ODataBatching8.Factories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ODataBatching8
{
    public class CustomODataBatchHandler : DefaultODataBatchHandler
    {
        private readonly IConfiguration _config;
        public CustomODataBatchHandler(IConfiguration configuratiuon)
        {
            _config = configuratiuon;
        }

        public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
        {
            if (requests == null)
            {
                throw new Exception(); //Error.ArgumentNull(nameof(requests));
            }

            if (handler == null)
            {
                throw new Exception(); //Error.ArgumentNull(nameof(handler));
            }

            

            IList<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
                              
            foreach (ODataBatchRequestItem request in requests)
            {
                using (var transaction = BookContextFactory.Create(DbContextType.SqlServer, _config.GetConnectionString("BookDatabase")).Database.BeginTransaction())
                {
                    ODataBatchResponseItem responseItem = await request.SendRequestAsync(handler).ConfigureAwait(false);
                    responses.Add(responseItem);

                    var wasSuccessful = responses.OfType<ChangeSetResponseItem>()
                        .Select(r => r.Contexts.All(c => c.Response.IsSuccessStatusCode()))
                        .All(c => c);

                    if (responseItem != null && wasSuccessful)// && responseItem.IsResponseSuccessful() == false && ContinueOnError == false)
                    {
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
            }
            
            return responses;
        }

    }
}