
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

using Vit.DynamicCompile.EntityGenerate;


namespace Vit.DynamicCompile.MsTest
{

    [TestClass]
    public class EntityGenerator_Test
    {


        [TestMethod]
        public void Test()
        {
            // #1
            var typeDescriptor = new TypeDescriptor
            {
                assemblyName = "newAsm",
                moduleName = "Main",
                typeName = "App.Model.Entity.User"
            };

            // #2 Type Attribute
            typeDescriptor.AddAttribute<TableAttribute>(constructorArgs: new object[] { "User1" });

            // #3 properties
            {
                // add id
                typeDescriptor.AddProperty(
                     PropertyDescriptor.New<int>("id")
                    .AddAttribute<KeyAttribute>()
                    .AddAttribute<DatabaseGeneratedAttribute>(constructorArgs: new object[] { DatabaseGeneratedOption.Identity })
                    .AddAttribute<ColumnAttribute>(constructorArgs: new object[] { "id" }, propertyValues: new (string, object)[] { ("TypeName", "int") })
                );

                // add name
                typeDescriptor.AddProperty(
                     PropertyDescriptor.New<string>("name")
                    .AddAttribute<RequiredAttribute>()
                    .AddAttribute<ColumnAttribute>(constructorArgs: new object[] { "name" }, propertyValues: new (string, object)[] { ("TypeName", "varchar(1000)") })
                );

            }

            // #4 CreateType
            var entityType = EntityGenerator.CreateType(typeDescriptor);

            #region #5 Assert
            {
                // TableAttribute
                var tableName = entityType.GetCustomAttribute<TableAttribute>()?.Name;
                Assert.AreEqual("User1", tableName);
                var properties = entityType.GetProperties();

                // id
                var id = properties[0];
                Assert.AreEqual("id", id.Name);
                // id - DatabaseGenerated
                Assert.AreEqual(DatabaseGeneratedOption.Identity, id.GetCustomAttribute<DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption);
                // id - TypeName
                Assert.AreEqual("int", id.GetCustomAttribute<ColumnAttribute>().TypeName);

                // name
                var name = properties[1];
                Assert.AreEqual("name", name.Name);
                // name - Required
                Assert.IsNotNull(name.GetCustomAttribute<RequiredAttribute>());
                // name - TypeName
                Assert.AreEqual("varchar(1000)", name.GetCustomAttribute<ColumnAttribute>().TypeName);
            }
            #endregion

        }



    }
}
