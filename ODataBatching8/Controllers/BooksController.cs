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
        private readonly IDbContextFactory<BooksContext> dbContextFactory;
        private BooksContext dbContext;

        public BooksController(IDbContextFactory<BooksContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;            
            //this.dbContext = booksContext;
        }

        [EnableQuery]        
        public async Task<IActionResult> Get()
        {
            dbContext = this.dbContextFactory.CreateDbContext();
            return Ok(await dbContext.Books.ToListAsync());
        }

        [EnableQuery]
        public async Task<IActionResult> Get([FromODataUri] Guid key)
        {
            dbContext = this.dbContextFactory.CreateDbContext();
            return Ok(await dbContext.Books.Where(x=>x.Id == key).FirstOrDefaultAsync());
        }
        [HttpPost]
        public async Task<IActionResult> Post(Book insert)
        {
            return Ok(await dbContext.Books.AddAsync(insert));
        }

        [HttpPatch]
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<Book> delta)
        {
            dbContext = this.dbContextFactory.CreateDbContext();
            var previousEntity = await dbContext.Books.FindAsync(key);
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
            dbContext = this.dbContextFactory.CreateDbContext();
            var deletingBook = await dbContext.Books.FindAsync(key);
            if (deletingBook == null)
            {
                return NotFound();
            }

            return Ok(dbContext.Books.Remove(deletingBook));
        }


    }
}
