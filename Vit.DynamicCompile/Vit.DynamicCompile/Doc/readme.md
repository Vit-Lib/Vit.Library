请在配置文件中 加
  <PropertyGroup>
    <PreserveCompilationContext>true</PreserveCompilationContext>
  </PropertyGroup>


https://github.com/lukencode/FluentEmail/issues/126
如果使用MvcRazor则加
  <PropertyGroup>
    <PreserveCompilationContext>true</PreserveCompilationContext>
	<MvcRazorExcludeRefAssembliesFromPublish>false</MvcRazorExcludeRefAssembliesFromPublish>
  </PropertyGroup>



