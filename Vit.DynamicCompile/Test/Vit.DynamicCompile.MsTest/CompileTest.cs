
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vit.DynamicCompile.MsTest
{
    public class ModelA
    {
        public int value;
        public int result;
    }


    [TestClass]
    public class CompileTest
    {

        #region (x.1)RunCodeBlockTest

        [TestMethod]
        public void RunCodeBlockTest()
        {

            try
            {
                string codeBlock = @" 
Action<Vit.DynamicCompile.MsTest.ModelA> action=(m)=>{ m.result = m.value + (int)args[0]; };
return action;
";

                var args = new object[] { 100 };

                var action = CSharpCompiler.RunCodeBlock(
                   new[] { Assembly.GetExecutingAssembly().Location },
                    codeBlock, 100)
                    as Action<Vit.DynamicCompile.MsTest.ModelA>;

                var m = new ModelA { value = 10 };
                Assert.IsNotNull(action);

                action(m);
                Assert.AreEqual(m.result, 110);
            }
            catch
            {
                throw;
            }
        }
        #endregion



        #region (x.2)CompileToAssemblyTest

        [TestMethod]
        public void CompileToAssemblyTest()
        {

            try
            {
                string testClass = @"using System; 
              namespace test{
               public class tes
               {
                    public static Action<Vit.DynamicCompile.MsTest.ModelA> getAction()
                    {
                        return (m)=>{ m.result = m.value + 1; };
                    }
               }
              }";


                var assembly = CSharpCompiler.CompileToAssembly(
                    new[] { Assembly.GetExecutingAssembly().Location },
                    testClass);

                var type = assembly.GetType("test.tes");

                var method = type?.GetMethod("getAction");
                var action = method?.Invoke(null, null) as Action<ModelA>;

                Assert.IsNotNull(action);


                var m = new ModelA { value = 121 };
                action(m);
                Assert.AreEqual(m.result, 122);
            }
            catch
            {
                throw;
            }

        }
        #endregion
    }
}
