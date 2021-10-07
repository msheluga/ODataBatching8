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
    public class UsersController : ControllerBase
    {
        private readonly BooksContext dbContext;

        public UsersController(BooksContext dbContext)
        {
            this.dbContext = dbContext;
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
