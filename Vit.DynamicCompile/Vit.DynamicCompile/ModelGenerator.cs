using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;

namespace Vit.Linq.ExpressionTree.ExpressionConvertor
{
    /*
// https://www.cnblogs.com/sesametech-dotnet/p/13176329.html
// https://learn.microsoft.com/zh-cn/dotnet/api/system.reflection.emit.constructorbuilder?view=net-8.0

<ItemGroup>
    <PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
    <PackageReference Include="System.Reflection.Emit" Version="4.7.0" />
</ItemGroup>
     */

    public class ModelGenerator
    {
        public static Type CreateType(Dictionary<string, Type> properties)
        {
            // #1 define Type 
            var assName = Guid.NewGuid().ToString();
            var moduleName = "Main";
            var typeName = "DynamicClass";
            var assemblyName = new AssemblyName(assName);
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule(moduleName);
            var typeBuilder = dynamicModule.DefineType(typeName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);     // This is the type of class to derive from. Use null if there isn't one
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public |
                                                MethodAttributes.SpecialName |
                                                MethodAttributes.RTSpecialName);

            // #2 Add Property
            //string propertyName = null;
            //Type propertyType = null;
            //AddProperty(dynamicType, propertyName, propertyType);
            foreach (var kv in properties)
            {
                AddProperty(typeBuilder, kv.Key, kv.Value);
            }

            var generatedType = typeBuilder.CreateTypeInfo().AsType();
            return generatedType;
        }
        private static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            var getMethod = typeBuilder.DefineMethod("get_" + propertyName,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getMethodIL = getMethod.GetILGenerator();
            getMethodIL.Emit(OpCodes.Ldarg_0);
            getMethodIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getMethodIL.Emit(OpCodes.Ret);

            var setMethod = typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });
            var setMethodIL = setMethod.GetILGenerator();
            Label modifyProperty = setMethodIL.DefineLabel();
            Label exitSet = setMethodIL.DefineLabel();

            setMethodIL.MarkLabel(modifyProperty);
            setMethodIL.Emit(OpCodes.Ldarg_0);
            setMethodIL.Emit(OpCodes.Ldarg_1);
            setMethodIL.Emit(OpCodes.Stfld, fieldBuilder);
            setMethodIL.Emit(OpCodes.Nop);
            setMethodIL.MarkLabel(exitSet);
            setMethodIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethod);
            propertyBuilder.SetSetMethod(setMethod);
        }
    }
}
