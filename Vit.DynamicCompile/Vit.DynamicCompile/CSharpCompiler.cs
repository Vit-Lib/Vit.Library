using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Vit.DynamicCompile
{
    // 参考 https://www.zhihu.com/question/268784285



    /// <summary> 
    /// 请在配置文件中 加
    /// &lt;PropertyGroup&gt;
    ///   &lt;PreserveCompilationContext&gt;true&lt;/PreserveCompilationContext&gt;
    /// &lt;/PropertyGroup&gt;   
    /// </summary>
    public class CSharpCompiler
    {
        #region Compile

        /// <summary>
        /// 编译代码。若编译不通过则抛异常
        /// </summary>
        /// <param name="OutputAssemblyStream">编译链接库的输出流</param>
        /// <param name="referencedAssemblies">依赖项。例如new[]{"a.dll","C:\\Program Files\\dotnet\\sdk\\NuGetFallbackFolder\\microsoft.netcore.app\/2.1.0\\ref\/netcoreapp2.1\/Microsoft.CSharp.dll"}</param>
        /// <param name="sources">源代码</param>
        public static void Compile(Stream OutputAssemblyStream, string[] referencedAssemblies, params string[] sources)
        {
            //(x.1)
            var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString() + ".dll")
                .WithOptions(new CSharpCompilationOptions(
                    Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
                    usings: null,
                    optimizationLevel: OptimizationLevel.Release, // TODO
                    checkOverflow: false,                       // TODO
                    allowUnsafe: true,                          // TODO
                    platform: Platform.AnyCpu,
                    warningLevel: 4,
                    xmlReferenceResolver: null // don't support XML file references in interactive (permissions & doc comment includes)
                    ));

            //(x.2)
            {
                //(x.x.1) default references
                var referencePaths = DependencyContext.Default.CompileLibraries.SelectMany(lib => lib.ResolveReferencePaths());

                //(x.x.2)user references
                if (referencedAssemblies != null) referencePaths = referencePaths.Union(referencedAssemblies);

                //(x.x.3)
                MetadataReference[] _ref =
                 referencePaths.Distinct()
               .Select(asm => MetadataReference.CreateFromFile(asm))
               .ToArray();
                compilation = compilation.AddReferences(_ref);


                //MetadataReference[] ref_ =
                //DependencyContext.Default.CompileLibraries
                //.First(cl => cl.Name == "Microsoft.NETCore.App")
                //.ResolveReferencePaths()
                //.Select(asm => MetadataReference.CreateFromFile(asm))
                //.ToArray();

            }



            //(x.3) parse source
            foreach (var source in sources)
            {
                compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source));
            }

            //(x.4)Emit
            var eResult = compilation.Emit(OutputAssemblyStream);
            if (!eResult.Success)
            {
                string errorMessage = "";
                foreach (var item in eResult.Diagnostics)
                {
                    errorMessage += item.GetMessage() + Environment.NewLine;
                }
                var ex = new Exception(errorMessage);
                ex.Data["CompilerError"] = eResult.Diagnostics;
                throw ex;
            }
        }
        #endregion

        #region Compile
        /// <summary>
        /// 编译代码。若编译不通过则抛异常
        /// </summary>
        /// <param name="referencedAssemblies">依赖项。例如new[]{"a.dll","C:\\Program Files\\dotnet\\sdk\\NuGetFallbackFolder\\microsoft.netcore.app\/2.1.0\\ref\/netcoreapp2.1\/Microsoft.CSharp.dll"}</param>
        /// <param name="sources">源代码</param>
        /// <returns></returns>
        public static byte[] Compile(string[] referencedAssemblies, params string[] sources)
        {

            using (MemoryStream stream = new MemoryStream())
            {
                Compile(stream, referencedAssemblies, sources);
                return stream.ToArray();
            }

        }

        /// <summary>
        /// 编译代码。若编译不通过则抛异常
        /// </summary>
        /// <param name="OutputAssemblyPath"></param>
        /// <param name="referencedAssemblies">依赖项。例如new[]{"a.dll","C:\\Program Files\\dotnet\\sdk\\NuGetFallbackFolder\\microsoft.netcore.app\/2.1.0\\ref\/netcoreapp2.1\/Microsoft.CSharp.dll"}</param>
        /// <param name="sources">源代码</param>
        public static void Compile(string OutputAssemblyPath, string[] referencedAssemblies, params string[] sources)
        {           
            using (FileStream stream = File.Open(OutputAssemblyPath, FileMode.Create))
            {
                Compile(stream, referencedAssemblies, sources);
                //stream.Close();//关闭流
            }            
        }
        /// <summary>
        ///  编译代码。若编译不通过则抛异常
        /// </summary>
        /// <param name="referencedAssemblies"></param>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static Assembly CompileToAssembly(string[] referencedAssemblies, params string[] sources)
        {  
            var data = Compile(referencedAssemblies, sources);
            if (null == data || data.Length == 0) return null;
            return Assembly.Load(data);
        }
        #endregion


        #region LoadAssemblyInMemory
        public static Assembly LoadAssemblyToMemory(string dllFile)
        {
            //先将插件拷贝到内存缓冲
            byte[] addinStream = File.ReadAllBytes(dllFile);
            //加载内存中的Dll
            return Assembly.Load(addinStream);
        }
        #endregion


        #region RunCodeBlock
        /// <summary>
        /// 编译并执行代码。若编译不通过或执行出错则抛异常
        /// </summary>
        /// <param name="referencedAssemblies">依赖项。例如new[]{"a.dll","C:\\Program Files\\dotnet\\sdk\\NuGetFallbackFolder\\microsoft.netcore.app\/2.1.0\\ref\/netcoreapp2.1\/Microsoft.CSharp.dll"}</param>
        /// <param name="codeBlock">代码。例如: " return args[0];" </param>
        /// <param name="args">参数。会传递到代码内部</param>
        /// <returns></returns>
        public static object RunCodeBlock(string[] referencedAssemblies, string codeBlock, params object[] args)
        {
            codeBlock = @"
using System;
namespace CodeCompilerCache
{
    public class StaticClass
    {
        public static object Func(object[] args)
        {
            " + codeBlock + @"
            return  null;
        }
    }
}";
            return CompileToAssembly(referencedAssemblies, codeBlock).GetType("CodeCompilerCache.StaticClass").GetMethod("Func").Invoke(null, new object[] { args });
        }
        #endregion
    }
}
