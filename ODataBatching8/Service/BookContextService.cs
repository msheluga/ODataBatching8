using Humanizer;
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

            var permissions = dbContextFactory.Permissions.Where(x => Guid.Equals(x.UserId, testUserId)).ToList();

            //GetModel(permissions, oDataModelBuilder);
            
            //return oDataModelBuilder.GetEdmModel();
            GetModel(permissions, model);
            return model;
        }
        private static void GetModel(List<Permission> permissions, ODataConventionModelBuilder oDataConventionModel)
        {
            const string EdmNamespaceName = "ODataBatching8.Models";
            permissions.OrderBy(x=>x.TableName).GroupBy(x => x.TableName)
                .ToList()                
                .ForEach(table =>
                {
                    if (table.All(x => x.TableAccessLevel > 0))
                    {
                        var tableType = Type.GetType(EdmNamespaceName + "." + table.Key);
                        
                        var typeConfig = oDataConventionModel.AddEntityType(tableType);
                        var tableTypeProp = tableType.GetProperties();

                        foreach (var prop in tableTypeProp)
                        {

                            if (table.All(x => x.TableAccessLevel == 0))
                            {
                                typeConfig.RemoveProperty(prop);
                            }
                        }
                        //remove the EntitySet
                        //oDataConventionModel.RemoveEntitySet(table.Key);
                        //oDataConventionModel.RemoveStructuralType(tableType);
                        oDataConventionModel.AddEntitySet(table.Key.Pluralize(), oDataConventionModel.AddEntityType(tableType));
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
                        EdmEntityType edmType = new EdmEntityType(EdmNamespaceName, table.Key);


                        foreach (var field in table.OrderBy(x => x.FieldOrder))
                        {
                            if (field.FieldAccessLevel > 0)
                            {
                                if (!String.IsNullOrEmpty(field.FieldProperties) && field.FieldProperties.Equals("Key"))
                                {
                                    EdmStructuralProperty idProp = edmType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Guid, false);
                                    edmType.AddKeys(idProp);
                                }
                                else if (!String.IsNullOrEmpty(field.FieldProperties) && field.FieldProperties.Equals("ForeignKey"))
                                {
                                    //would need to check multiple properties to see what I have like colleciton or such 
                                    var fkEntity = model.AddEntityType(EdmNamespaceName, field.FieldName);
                                    var fkId = fkEntity.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Guid);
                                    var allowedFKId = edmType.AddStructuralProperty(field.FieldName + "Id", EdmPrimitiveTypeKind.Guid);
                                    fkEntity.AddKeys(fkId);
                                    var navFkEntity = edmType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo()
                                    {
                                        Name = field.FieldName,
                                        TargetMultiplicity = EdmMultiplicity.One,
                                        Target = fkEntity,
                                        DependentProperties = new IEdmStructuralProperty[] { allowedFKId }, //Ref constraint Property
                                        PrincipalProperties = new IEdmStructuralProperty[] { fkId }, //Ref ReferencedProperty 
                                    });
                                }
                                else if (!String.IsNullOrEmpty(field.FieldProperties) && field.FieldProperties.Equals("Collection"))
                                {
                                    var fkEntity = model.AddEntityType(EdmNamespaceName, field.FieldName);
                                    var navFkEntity = edmType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo()
                                    {
                                        Name = field.FieldName,
                                        TargetMultiplicity = EdmMultiplicity.Many,
                                        Target = fkEntity,
                                    });
                                }
                                else
                                {
                                    var edmValue = EdmTypeUtil.DbTypeToEdmType(field.FieldDataType);
                                    edmType.AddStructuralProperty(field.FieldName, edmValue.Value);
                                }
                            }
                        }
                        model.AddElement(edmType);
                        container.AddEntitySet(table.Key.Pluralize(), edmType);
                    }

                });

            model.AddElement(container);

        }
    }
}
