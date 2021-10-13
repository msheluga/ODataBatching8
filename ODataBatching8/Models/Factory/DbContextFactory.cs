using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataBatching8.Models.Factory
{
    public enum DBContextType
    {
        Undefined, 
        SqlServer,
        InMemory
    }

    public class DbContextFactory
    {
        public static BooksContext Create(DBContextType type, string connectionString = null)
        {
            return type switch
            {
                DBContextType.SqlServer => new BooksContext(new DbContextOptionsBuilder<BooksContext>().UseSqlServer(connectionString).Options),
                //DBContextType.InMemory =>  new BooksContext(new DbContextOptionsBuilder<BooksContext>().UseInMemoryDatabase("Books").Options),                                          
                _ => null
            };
        }
    }
}
