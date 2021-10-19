using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using ODataBatching8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataBatching8.Controllers
{
    public class UsersController : ControllerBase
    {
        private readonly IDbContextFactory<BooksContext> dbContextFactory;
        private BooksContext dbContext;

        public UsersController(IDbContextFactory<BooksContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
            dbContext = this.dbContextFactory.CreateDbContext();
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(dbContext.Users);
        }

        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] Guid key)
        {
            return Ok(await dbContext.Users.FindAsync(key));
        }
    }
}
