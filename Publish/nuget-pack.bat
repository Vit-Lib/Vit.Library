@echo off

::(x.1)��ȡbasePath
cd /D %~dp0
cd /d .. 
set basePath=%cd%
set nugetPath=%cd%\Publish\nuget

::(x.2)����������Ҫ����nuget����Ŀ������
for /f "delims=" %%f in ('findstr /M /s /i "<pack/>" *.csproj') do (
	echo pack %basePath%\%%f\..
	cd /d %basePath%\%%f\.. 
	dotnet build --configuration Release
	dotnet pack --configuration Release --output %nugetPath%
	@if errorlevel 1 (echo . & echo .  & echo �������Ų飡& pause) 
)

cd /d %basePath%\Publish

echo 'pack nuget succeed��'
echo 'pack nuget succeed��'

 