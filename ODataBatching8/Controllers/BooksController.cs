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
    public class BooksController : ControllerBase
    {
        private readonly BooksContext dbContext;

        public BooksController(BooksContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [EnableQuery]        
        public IActionResult Get()
        {
            return Ok(dbContext.Book);
        }

        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] Guid key)
        {
            return Ok(await dbContext.Book.Where(x=>x.Id == key).FirstOrDefaultAsync());
        }

    }
}
