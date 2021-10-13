using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using ODataBatching8.Models;
using ODataBatching8.Models.Factory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;

namespace ODataBatching8
{
    public class CustomODataBatchHandler : DefaultODataBatchHandler
    {
        private readonly IConfiguration _config;
        public CustomODataBatchHandler(IConfiguration configuratiuon)
        {
            _config = configuratiuon;
        }

        

        public async override Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(
           IEnumerable<ODataBatchRequestItem> requests,
           RequestDelegate handler)
        {
            if (requests == null)
            {
                throw new ArgumentNullException("requests");
            }

            IList<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            try
            {
                foreach (ODataBatchRequestItem request in requests)
                {
                    var operation = request as OperationRequestItem;
                    if (operation != null)
                    {
                        responses.Add(await request.SendRequestAsync(handler));
                    }
                    else
                    {
                        await ExecuteChangeSet((ChangeSetRequestItem)request, handler, responses);
                    }
                }
            }
            catch
            {
                foreach (ODataBatchResponseItem response in responses)
                {
                    if (response != null)
                    {
                        throw;
                    }
                }
                throw;
            }

            return responses;
        }

        private async Task ExecuteChangeSet(ChangeSetRequestItem changeSet, RequestDelegate handler, IList<ODataBatchResponseItem> responses)
        {

            

            using var context = DbContextFactory.Create(DBContextType.SqlServer, _config.GetConnectionString("BookDatabase"));

            //Add the dbcontext into all of the changeset context
            foreach (var request in changeSet.Contexts)
            {
                request.Items.TryAdd("Books_DBContext", context);
            }

            using var transaction = context.Database.BeginTransaction();

            var changeSetResponse = (ChangeSetResponseItem)await changeSet.SendRequestAsync(handler);

            if (changeSetResponse.Contexts.Any(r => r.Response.StatusCode != 200))
            {
                throw new Exception("Transaction failed"); 
            }
            responses.Add(changeSetResponse);

            if (changeSetResponse.Contexts.All(r => r.Response.StatusCode == 200))
            {
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }



        }
    }
}