# please add below config to csproj
``` csproj
<PropertyGroup>
    <PreserveCompilationContext>true</PreserveCompilationContext>
  </PropertyGroup>
```


# add below config to csproj if use MvcRazor
> https://github.com/lukencode/FluentEmail/issues/126
``` csproj
  <PropertyGroup>
    <PreserveCompilationContext>true</PreserveCompilationContext>
    <MvcRazorExcludeRefAssembliesFromPublish>false</MvcRazorExcludeRefAssembliesFromPublish>
  </PropertyGroup>
```

