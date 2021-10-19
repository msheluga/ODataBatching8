using Microsoft.AspNetCore.Http;
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
    
    public class GroupsController : ControllerBase
    {
        private readonly IDbContextFactory<BooksContext> dbContextFactory;
        private BooksContext dbContext;

        public GroupsController(IDbContextFactory<BooksContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
            dbContext = this.dbContextFactory.CreateDbContext();
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(dbContext.Groups);
        }

        [EnableQuery]
        public IActionResult Get([FromODataUri] Guid key)
        {
            return Ok(dbContext.Groups.Find(key));
        }
    }
}
