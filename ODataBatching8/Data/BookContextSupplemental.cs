//using EFSecurity.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace ODataBatching8.Data
{
    public partial class BooksContext : DbContext
    {
        public 

       // private readonly IHttpContextAccessor _ca;

        //    public BooksContext(DbContextOptions<BooksContext> options, IHttpContextAccessor contextAccessor)
        //        : base(options)
        //    {
        //        _ca = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        //    }

        //    protected void OnModelCreatingPartial(ModelBuilder modelBuilder)
        //    {
        //        //call the security model and get the permissions and store them 
        //        //get the context
        //        var config = new ConfigurationBuilder()
        //            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        //            .AddJsonFile("appsettings.json").Build();
        //        DbContextOptions<PermissionContext> options = new DbContextOptionsBuilder<PermissionContext>()
        //        .UseSqlServer(config.GetConnectionString("PermissionDatabase"))
        //        .Options;
        //        var permissionContext = new PermissionContext(options);
        //        //get the permissions 
        //        //TODO: need to test this  
        //        #region ClaimsSection 
        //        //var userId = _ca?.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        //        //var permissions = context.AspNetUserClaims.AsNoTracking().Where(x => x.UserId == userId);
        //        //foreach (var claim in permissions.Where(x => !x.ClaimType.Contains('.')))
        //        //{
        //        //    var table = claim.ClaimType;
        //        //    var entity = modelBuilder.Entity(table);
        //        //    var fieldsPermissions = permissions.Where(x => x.ClaimType.Contains(table) && x.ClaimType.Contains('.'));
        //        //    foreach (var field in fieldsPermissions.Where(x => x.ClaimType.Contains("Read")))
        //        //    {
        //        //        if (field.ClaimValue.ToLower() == "false")
        //        //        {
        //        //            entity.Ignore(field.ClaimType.Split('.')[1]);
        //        //        }
        //        //    }
        //        //}
        //        #endregion
        //        #region customDbSolution
        //        var e920Tables = permissionContext.ApplicationPermissionGroups
        //            .SelectMany(pg => pg.PermissionGroup.PermissionGroupE920tables) // All the tables from all the permission groups
        //            .Where(pgt => pgt.AccessGet && !pgt.DeletedDate.HasValue) // Filter out ones without 'GET' permission
        //            .GroupBy(pgt => pgt.E920table.Name) // group tables by name because multiple permission groups could have the same table with different field permissions
        //            .Select(group => new
        //            {
        //                TableName = group.Key,
        //                AllowedFields = group
        //                    //.SelectMany(g => g.PermissionGroupE920tableFields) // grab all the field permissions for the group
        //                   // .Where(pgtf => pgtf.Get && !pgtf.DeletedDate.HasValue) // filter out ones that dont have 'GET' Permission
        //                    //.Select(pgtf => pgtf.E920tableField.FieldName.ToLower()) // All we need is the name
        //            })
        //            .OrderBy(t => t.TableName)                
        //            .ToList(); 

        //        foreach (var table in e920Tables)
        //        {
        //            var key = table.TableName;
        //            var fields = table.AllowedFields.ToList();
        //            foreach (var field in fields.Select(x=>x.PermissionGroupE920tableFields))
        //            {

        //            }
        //        }

        //        #endregion
        //    }       
    }
}
