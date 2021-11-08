using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using ODataBatching8.Models;
using ODataBatching8.Service;
using System;

namespace ODataBatching8
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var customBatchHandler = new CustomODataBatchHandler(Configuration);
            customBatchHandler.MessageQuotas.MaxOperationsPerChangeset = 10;
            customBatchHandler.MessageQuotas.MaxPartsPerBatch = 10;
            customBatchHandler.MessageQuotas.MaxNestingDepth = 2;

            services.AddDbContextFactory<BooksContext>(opt =>
                opt.UseSqlServer(Configuration.GetConnectionString("BookDatabase")));
           

            services.AddControllers().AddOData(opt=>
                opt.Select().Filter().Count().OrderBy().Expand().EnableQueryFeatures()
                .AddRouteComponents("odata", BooksContextService.GetEdmModel(Configuration.GetConnectionString("BookDatabase")), customBatchHandler)
                //.AddRouteComponents("odata",GetEdmModel(), customBatchHandler)
            );

            services.AddCors();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ODataBatching8", Version = "v1" });
            });
        }

        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ODataBatching8 v1"));
            }

            app.UseODataQueryRequest();
            app.UseODataBatching();
            app.UseHttpsRedirection();
            app.UseODataRouteDebug();
            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();                
            });
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Book>("Books");
            //builder.EntitySet<Groups>("Groups");
            //builder.EntitySet<Users>("Users");
            var model = builder.GetEdmModel();
            return model;
        }

        //private static IEdmModel GetEdmModel()
        //{
        //    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        //    builder.Namespace = "WebAPI";
        //    builder.ContainerName = "DefaultContainer";
        //    builder.EnableLowerCamelCase();

        //    var booksContextService = new BooksContextService(Configuration.GetConnectionString("BookDatabase"));

        //    var permissionList = BooksContextService.GetPermissionSet();
        //    //foreach (Type item in GetTypesInNamespace(System.Reflection.Assembly.Load("ProjectDLL"), "NamespaceOfModels"))
        //    //{
        //    //    //My models have a key named "Id"
        //    //    if (item.GetProperty("Id") == null)
        //    //        continue;

        //    //    EntityTypeConfiguration entityType = builder.AddEntityType(item);
        //    //    entityType.HasKey(item.GetProperty("Id"));
        //    //    builder.AddEntitySet(item.Name, entityType);
        //    //}

        //    return builder.GetEdmModel();
        //}

    }

    
}
