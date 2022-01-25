using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using ODataBatching8.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODataBatching8.Data;

namespace ODataBatching8.Controllers
{
    public class AddressesController : ControllerBase
    {
        private readonly IDbContextFactory<BooksContext> dbContextFactory;
        private BooksContext dbContext;
        public AddressesController(IDbContextFactory<BooksContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            dbContext = this.dbContextFactory.CreateDbContext();
            return Ok(dbContext.Addresses);
        }

        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] Guid key)
        {
            dbContext = this.dbContextFactory.CreateDbContext();
            return Ok(await dbContext.Addresses.Where(x => x.Id == key).FirstOrDefaultAsync());
        }
    }
}