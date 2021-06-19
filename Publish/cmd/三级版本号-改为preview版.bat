@echo off

::��ȡ��ǰ�汾��
:: set version=2.1.3
for /f "tokens=3 delims=><" %%a in ('type ..\..\Vit.Db\Vit.Db\Vit.Db.csproj^|findstr "<Version>.*Version"') do set version=%%a

for /f "tokens=1 delims=-" %%i in ("%version%") do set numVersion=%%i

:: v1 v2 v3
for /f "tokens=1 delims=." %%i in ("%numVersion%") do set v1=%%i
for /f "tokens=2 delims=." %%i in ("%numVersion%") do set v2=%%i
for /f "tokens=3 delims=." %%i in ("%numVersion%") do set v3=%%i


:: set /a v3=1+%v3%
set  newVersion=%v1%.%v2%.%v3%-preview

 
echo �Զ��޸İ汾�� [%version%]-^>[%newVersion%]
echo.

:: ���ù��� �滻csproj�ļ��еİ汾��
VsTool.exe replace -r --path "..\.." --file "*.csproj" --old "%version%" --new "%newVersion%"


echo.
echo.
echo.
echo �Ѿ��ɹ��޸İ汾�� [%version%]-^>[%newVersion%]
pause