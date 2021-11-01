using Microsoft.AspNetCore.OData.Batch;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataBatching8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataBatching8.Service
{
    public static class BooksContextService
    {
        internal static IEdmModel GetEdmModel(string connectionString)
        {

            var model = new EdmModel();
            var container = model.EntityContainer;
            var oDataModelBuilder = new ODataConventionModelBuilder();

            BooksContext dbContextFactory = new BooksContext(new DbContextOptionsBuilder<BooksContext>()
                                            .UseSqlServer(connectionString).Options);

            var testUserId = new Guid("B726EA14-2A28-E9FE-D543-047FA58C6A82");

            var permissions = dbContextFactory.Permission.Where(x => Guid.Equals(x.UserId, testUserId)).ToList();

            GetModel(permissions, oDataModelBuilder);
            //GetModel(permissions, model);
            return oDataModelBuilder.GetEdmModel();
        }
        private static void GetModel(List<Permission> permissions, ODataConventionModelBuilder oDataConventionModel)
        {
            const string EdmNamespaceName = "ODataBatching8.Models";
            permissions.GroupBy(x => x.TableName)
                .ToList()
                .ForEach(table =>
                {
                    var tableType = Type.GetType(EdmNamespaceName + "." + table.Key);
                    //builder.AddEntitySet(t.Name, builder.AddEntityType(t));
                     var typeConfig = oDataConventionModel.AddEntityType(tableType);
                    var tableTypeProp = tableType.GetProperties();
                    if (table.All(x => x.TableAccessLevel > 0))
                    {   
                       
                        oDataConventionModel.AddEntitySet(table.Key, oDataConventionModel.AddEntityType(tableType));                        
                    }
                    else
                    {
                        foreach (var prop in tableTypeProp)
                        {
                            typeConfig.RemoveProperty(prop);                            
                        }
                        //remove the EntitySet
                        //ssoDataConventionModel.RemoveEntitySet(table.Key);
                        //oDataConventionModel.RemoveStructuralType(tableType);
                    }

                });
        }
            private static void GetModel(List<Permission> permissions, EdmModel model)
        {
            const string EdmNamespaceName = "ODataBatching8.Models";
            permissions.GroupBy(x => x.TableName)
                .ToList()
                .ForEach(table =>
                {

                    if (table.All(x => x.TableAccessLevel > 0))
                    {
                        EdmEntityType edmTable = new EdmEntityType(EdmNamespaceName, table.Key);
                        //var tableType = Type.GetType(EdmNamespaceName + "." + table.Key);
                        //builder.AddEntitySet(t.Name, builder.AddEntityType(t));
                        //var typeConfig = oDataConventionModel.AddEntityType(tableType);
                        //var tableTypeProp = tableType.GetProperties();
                        //oDataConventionModel.AddEntitySet(table.Key, oDataConventionModel.AddEntityType(tableType));
                        foreach (var field in table.OrderBy(x => x.FieldOrder))
                        {
                            if (field.FieldAccessLevel > 0)
                            {
                                //var edmValue = EdmTypeUtil.DbTypeToEdmType(field.FieldDataType);
                                //        //tableType.AddStructuralProperty(field.FieldName, edmValue.Value);
                                //oDataConventionModel.RemoveStructuralType(Type.GetType(field.FieldDataType));

                                //if (container.FindEntitySet(table.Key) == null)
                                //{                                   
                                //    //check other properties 
                                //    //navigation properties
                                //    if (field.FieldDataType.Contains("ForeignKey"))
                                //    {
                                //        //add navigation key
                                //         EdmNavigationProperty.CreateNavigationProperty(Type.GetType(field.FieldDataType), field.FieldName);
                                //        //add ref constraint 
                                //    }
                                //    else if (field.FieldDataType.Contains("Collection"))
                                //    {
                                //        //add navigation property 
                                //        //EdmStructuredType edmStructuredType = new EdmStructuredType()
                                //        //tableType.AddProperty(EdmNavigationProperty.CreateNavigationProperty(EdmPrimitiveTypeKind.))

                                //    }
                                //    else //add normal field 
                                //    {
                                //        //var edmValue = EdmTypeUtil.DbTypeToEdmType(field.FieldDataType);
                                //        //tableType.AddStructuralProperty(field.FieldName, edmValue.Value);
                                //    }
                                switch (field.FieldProperties)
                                {
                                    case "Key":
                                        edmTable.AddKeys(edmTable.AddStructuralProperty(field.FieldName, EdmPrimitiveTypeKind.Guid));
                                        break;
                                    case "ForeignKey":
                                        EdmNavigationProperty navProp = edmTable.AddUnidirectionalNavigation(
                                        new EdmNavigationPropertyInfo
                                        {
                                            Name = "Press",
                                            TargetMultiplicity = EdmMultiplicity.Many,
                                            Target = new EdmEntityType(EdmNamespaceName, "Press"),
                                        });
                                        edmTable.AddProperty(navProp);
                                        break;
                                    case "field":
                                        edmTable.AddStructuralProperty(field.FieldName, EdmTypeUtil.DbTypeToEdmType(field.FieldDataType).Value);
                                        break;
                                }
                                //    // meSingleton.AddNavigationTarget(propertyAirline, targetAirlines, new EdmPathExpression("Trips/PlanItems/ODataSamples.WebApiService.Models.Flight/Airline"));
                                //    //meSingleton.AddNavigationTarget(propertyFrom, targetAirports, new EdmPathExpression("Trips/PlanItems/ODataSamples.WebApiService.Models.Flight/From"));
                                //    //meSingleton.AddNavigationTarget(propertyTo, targetAirports, new EdmPathExpression("Trips/PlanItems/ODataSamples.WebApiService.Models.Flight/To"));
                                //}                                
                            }

                        }
                        //model.AddElement(tableType);
                        //container.AddEntitySet(table.Key, tableType);
                    }
                });
        }
    }
}
