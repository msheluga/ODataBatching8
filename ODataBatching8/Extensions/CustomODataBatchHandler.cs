using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.OData.Extensions;
using System.Linq;
using ODataBatching8.Models;
using System.Transactions;

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
                if (request is OperationRequestItem operation)
                {
                    responses.Add(await request.SendRequestAsync(handler));
                }
                else
                {
                    responses.Add(await ExecuteChangeSet((ChangeSetRequestItem)request, responses, handler));
                }
                
            }
            
            return responses;
        }

        private async Task<ODataBatchResponseItem> ExecuteChangeSet(ChangeSetRequestItem request, IList<ODataBatchResponseItem> responses, RequestDelegate handler)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            var response = (ChangeSetResponseItem)await request.SendRequestAsync(handler);
            if (response.Contexts.All(c => c.Response.StatusCode >= 200 && c.Response.StatusCode < 300))
            {
                scope.Complete();
            }
            return response;
        }
    }
}