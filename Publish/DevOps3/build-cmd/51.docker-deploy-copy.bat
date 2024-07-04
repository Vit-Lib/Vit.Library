@echo off


echo %~n0.bat start...


::#1 get basePath
set curPath=%cd%
cd /d "%~dp0"
cd /d ..\..\..
set basePath=%cd%


:: #2
set publishPath=%basePath%\Publish\release\release\Station(net6.0)
set dockerPath=%basePath%\Publish\release\release\docker-deploy
rd /s /q "%dockerPath%"


:: #3 copy dir
xcopy "%basePath%\Publish\ReleaseFile\docker-deploy" "%dockerPath%" /e /i /r /y


:: #4 copy station 
xcopy "%publishPath%\ServiceCenter\appsettings.json" "%dockerPath%\sers"
xcopy "%publishPath%\Gateway\appsettings.json" "%dockerPath%\sers-gateway"
xcopy "%publishPath%\Gover\appsettings.json" "%dockerPath%\sers-gover"
xcopy "%publishPath%\Demo\appsettings.json" "%dockerPath%\sers-demo"
xcopy "%publishPath%\Robot\appsettings.json" "%dockerPath%\sers-demo-robot"
xcopy "%basePath%\Publish\release\release\压测\单体压测net6.0\ServiceCenter\appsettings.json" "%dockerPath%\sers-demo-sersall"




echo %~n0.bat success
cd /d "%curPath%"