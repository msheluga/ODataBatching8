using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using ODataBatching8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataBatching8.Controllers
{
    
    public class GroupsController : ControllerBase
    {
        private readonly BooksContext dbContext;

        public GroupsController(BooksContext dbContext)
        {
            this.dbContext = dbContext;
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
