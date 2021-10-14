using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Transactions;
using Microsoft.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


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
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    ODataBatchResponseItem responseItem = await request.SendRequestAsync(handler).ConfigureAwait(false);
                    responses.Add(responseItem);

                    if (responseItem != null)// && responseItem.IsResponseSuccessful() == false && ContinueOnError == false)
                    {
                        break;
                    }
                }
            }
            return responses;
        }

    }
}