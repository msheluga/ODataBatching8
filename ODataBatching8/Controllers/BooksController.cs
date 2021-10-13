using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
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
        [HttpPost]
        public async Task<IActionResult> Post(Book insert)
        {
            return Ok(await dbContext.Book.AddAsync(insert));
        }

        [HttpPatch]
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<Book> delta)
        {
            var previousEntity = await dbContext.Book.FindAsync(key);
            if (previousEntity == null)
            {
                return NotFound();
            }
            delta.Patch(previousEntity);
            return Ok(await dbContext.SaveChangesAsync());
        }
        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] Guid key)
        {
            var deletingBook = await dbContext.Book.FindAsync(key);
            if (deletingBook == null)
            {
                return NotFound();
            }

            return Ok(dbContext.Book.Remove(deletingBook));
        }


    }
}
