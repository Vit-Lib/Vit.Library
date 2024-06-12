
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using Vit.Linq.ExpressionTree.ExpressionConvertor;

namespace Vit.DynamicCompile.MsTest
{

    [TestClass]
    public class ModelGenerator_Test
    {

        public class ModelB
        {
            public object value { get; set; }
            public int result { get; set; }
        }


        [TestMethod]
        public void Test()
        {
            var properties = typeof(ModelB).GetProperties().ToDictionary(property => property.Name, property => property.PropertyType);

            var newType = ModelGenerator.CreateType(properties);

            var newProperties = newType.GetProperties();

            Assert.AreEqual(2, newProperties.Count());

        }




    }
}
