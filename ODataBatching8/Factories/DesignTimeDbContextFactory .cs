using Microsoft.EntityFrameworkCore;
using ODataBatching8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ODataBatching8.Factories
{
    public enum DbContextType
    {
        Undefined,
        SqlServer,
        InMemory
    }

    public class BookContextFactory
    {
        public static BooksContext Create(DbContextType type, string connectionString = null)
        {
            return type switch
            {
                DbContextType.SqlServer =>
                                    new BooksContext(new DbContextOptionsBuilder<BooksContext>()
                                            .UseSqlServer(connectionString).Options),
                //DbContextType.InMemory =>
                //                    new BooksContext(new DbContextOptionsBuilder<BooksContext>()
                //                            .UseInMemoryDatabase("Books").Options),
                _ => null,
            };
        }
    }
}
