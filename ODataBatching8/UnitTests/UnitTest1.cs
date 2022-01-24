using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ODataBatching8.Controllers;
using ODataBatching8.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace OData8BatchingUnitTest
{
    public class BookControllerUnitTest
    {
        public BooksContext BooksContext { get; private set; }

        DbContextOptions<BooksContext> options = new DbContextOptionsBuilder<BooksContext>()
            .UseInMemoryDatabase("BookDatabase")
            .Options;

        private BooksController booksController;
        Guid id1 = Guid.NewGuid();

        public BookControllerUnitTest()
        {
            SeedDb();
            booksController = new BooksController((IDbContextFactory<BooksContext>)options);
        }

        private void SeedDb()
        {
            using var context = new BooksContext(options);
            var books = new List<Book>
            {               
               // book #1
               new Book
                {
                    Id = id1,
                    Isbn = "978-0-321-87758-1",
                    Title = "Essential C#5.0",
                    Author = "Mark Michaelis",
                    Price = 59.99m,
                    //Location = new Address { City = "Redmond", Street = "156TH AVE NE" },
                    Press = new Press
                    {
                        Id = Guid.NewGuid(),
                        Name = "Addison-Wesley",
                        Category = 2
                    }
                },

               // book #2
               new Book
                {
                    Id = Guid.NewGuid(),
                    Isbn = "063-6-920-02371-5",
                    Title = "Enterprise Games",
                    Author = "Michael Hugos",
                    Price = 49.99m,
                    //Location = new Address { City = "Bellevue", Street = "Main ST" },
                    Press = new Press
                    {
                        Id = Guid.NewGuid(),
                        Name = "O'Reilly",
                        Category = 1,
                    }
                },
            };
            context.Books.AddRange(books);
            context.SaveChanges();
        }

        [Test]
        public async Task Get_FetchedBooksWithoutAddressesandLocations()
        {
            using var context = new BooksContext(options);
            var actionResult = await booksController.Get();

            var okResult = actionResult as OkObjectResult;
            var actualBooks = okResult.Value as List<Book>;
            actualBooks.Count.Should().Be(2);

        }

        [Test]
        public async Task Get_FetchById()
        {
            using var context = new BooksContext(options);
            var result = await booksController.Get(id1);
            var okResult = result as OkObjectResult;
            var book = okResult.Value.As<Book>();

            book.Should().NotBeNull();
            book.Press.Should().NotBeNull();
        }
    }
}