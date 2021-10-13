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

        public override async Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await base.ProcessBatchAsync(context, nextHandler);
            }
        }


    }
}