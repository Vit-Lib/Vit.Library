@echo off

::enable delayed arguments
setlocal EnableDelayedExpansion



:: #1 get ��ȡbasePath
set curPath=%cd%
cd /d "%~dp0"
cd /d ../../..
set basePath=%cd%



:: #2
set publishPath=%basePath%/Publish/release/release/StressTest





echo ------------------------------------------------------------------
echo "#3 publish CL stressTest"

::Client
cd /d "%basePath%\dotnet\Library\Sers\Sers.CL\Test\CommunicationManage\CmClient"
dotnet build --configuration Release
dotnet publish --configuration Release --output "%publishPath%\CLѹ��net6.0\CmClient"
@if errorlevel 1 (echo . & echo .  & echo error & pause) 

::Server
cd /d "%basePath%\dotnet\Library\Sers\Sers.CL\Test\CommunicationManage\CmServer"
dotnet build --configuration Release
dotnet publish --configuration Release --output "%publishPath%\CLѹ��net6.0\CmServer"
@if errorlevel 1 (echo . & echo .  & echo error & pause) 


::copy bat
xcopy  "%basePath%\Publish\ReleaseFile\StressTest\CLѹ��" "%publishPath%\CLѹ��net6.0" /e /i /r /y




echo ------------------------------------------------------------------
:: #4 publish Sers stressTest
for %%i in (net6.0) do (  
	set netVersion=%%i
	set appPath=%basePath%/Publish/release/release/Station^(!netVersion!^)

	echo ���� ѹ��-!netVersion!

	::����ѹ��
	set targetPath=%publishPath%/����ѹ��!netVersion!

	::(x.x.1)copy ServiceCenter
	xcopy "!appPath!\ServiceCenter" "!targetPath!\ServiceCenter" /e /i /r /y

	::(x.x.2)copy demo
	xcopy "!appPath!\Demo\wwwroot" "!targetPath!\ServiceCenter\wwwroot" /e /i /r /y
	xcopy "!appPath!\Demo\Did.SersLoader.Demo.dll" "!targetPath!\ServiceCenter" /i /r /y
	xcopy "!appPath!\Demo\Did.SersLoader.Demo.pdb" "!targetPath!\ServiceCenter" /i /r /y
	xcopy "!appPath!\Demo\Did.SersLoader.Demo.xml" "!targetPath!\ServiceCenter" /i /r /y

	::(x.x.3)copy Robot
	xcopy "!appPath!\Robot\wwwroot" "!targetPath!\ServiceCenter\wwwroot" /e /i /r /y
	xcopy "!appPath!\Robot\App.Robot.Station.dll" "!targetPath!\ServiceCenter" /i /r /y
	xcopy "!appPath!\Robot\App.Robot.Station.pdb" "!targetPath!\ServiceCenter" /i /r /y
	xcopy "!appPath!\Robot\App.Robot.Station.xml" "!targetPath!\ServiceCenter" /i /r /y

	::(x.x.4)copy ReleaseFile
	xcopy "%basePath%\Publish\ReleaseFile\StressTest\����ѹ��" "!targetPath!" /e /i /r /y



	::�ֲ�ʽѹ��
	set targetPath=%publishPath%/�ֲ�ʽѹ��!netVersion!

	::(x.x.1)copy  station
	xcopy "!appPath!\ServiceCenter" "!targetPath!\ServiceCenter" /e /i /r /y
	xcopy "!appPath!\Demo" "!targetPath!\Demo" /e /i /r /y
	xcopy "!appPath!\Robot" "!targetPath!\Robot" /e /i /r /y

	::(x.x.2)copy ReleaseFile
	xcopy  "%basePath%\Publish\ReleaseFile\StressTest\�ֲ�ʽѹ��" "!targetPath!" /e /i /r /y
)



 


echo %~n0.bat success
cd /d "%curPath%"



