cd VisualStudio

set PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin";%PATH%

call 01-build.bat
if %errorlevel% neq 0 exit /b %errorlevel%
call 02-tests.bat
if %errorlevel% neq 0 exit /b %errorlevel%
call 03-release.bat
if %errorlevel% neq 0 exit /b %errorlevel%
