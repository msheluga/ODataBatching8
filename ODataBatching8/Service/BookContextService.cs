using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataBatching8.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ODataBatching8.Service
{
    public static class BooksContextService
    {
        internal static IEdmModel GetEdmModel(string connectionString)
        {

            var model = new EdmModel();

            var oDataModelBuilder = new ODataConventionModelBuilder();

            BooksContext dbContextFactory = new BooksContext(new DbContextOptionsBuilder<BooksContext>()
                                            .UseSqlServer(connectionString).Options);

            var testUserId = new Guid("B726EA14-2A28-E9FE-D543-047FA58C6A82");

            var permissions = dbContextFactory.Permission.Where(x => Guid.Equals(x.UserId, testUserId)).ToList();

            //GetModel(permissions, oDataModelBuilder);
            GetModel(permissions, model);
            //return oDataModelBuilder.GetEdmModel();
            return model;
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
            EdmEntityContainer container = new EdmEntityContainer(EdmNamespaceName, "Container");
            permissions.GroupBy(x => x.TableName)
                .ToList()
                .ForEach(table =>
                {
                    if (table.All(x => x.TableAccessLevel > 0))
                    {
                        EdmEntityType edmType = new EdmEntityType(EdmNamespaceName + "." + table.Key, table.Key);
                        EdmStructuralProperty idProp = edmType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Guid, false);
                        edmType.AddKeys(idProp);

                        container.AddEntitySet(table.Key, edmType);
                        var allowedEntity = model.AddEntityType(EdmNamespaceName, table.Key);
                        foreach (var field in table)
                        {
                            if (field.FieldAccessLevel > 0)
                            {
                                if (!String.IsNullOrEmpty(field.FieldProperties) && field.FieldProperties.Contains("ForeignKey"))
                                {
                                    //would need to check multiple properties to see what I have like colleciton or such 
                                    var fkEntity = model.AddEntityType(EdmNamespaceName, field.FieldName);
                                    var fkId = fkEntity.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Guid);
                                    var allowedFKId = allowedEntity.AddStructuralProperty(field.FieldName + "Id", EdmPrimitiveTypeKind.Guid);
                                    fkEntity.AddKeys(fkId);
                                    var navFkEntity = allowedEntity.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo()
                                    {                                        
                                        Name = field.FieldName,
                                        TargetMultiplicity = EdmMultiplicity.One,
                                        Target = fkEntity,
                                        DependentProperties =new IEdmStructuralProperty[] { allowedFKId }, //Ref constraint Property
                                        PrincipalProperties = new IEdmStructuralProperty[] { fkId }, //Ref ReferencedProperty 
                                    });                                    
                                }                               
                                else
                                {
                                    var edmValue = EdmTypeUtil.DbTypeToEdmType(field.FieldDataType);
                                    allowedEntity.AddStructuralProperty(field.FieldName, edmValue.Value);
                                }
                            }
                        }
                    }

                });

            model.AddElement(container);

        }
    }
}
