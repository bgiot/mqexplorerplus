set version=1.2.0

set vswhere="%ProgramFiles(X86)%\Microsoft Visual Studio\Installer\vswhere.exe"

for /f "usebackq tokens=*" %%i in (`%vswhere% -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do  (
  set MSBUILD=%%i
)
  
mkdir artifacts

"%MSBUILD%" src\Dotc.MQExplorerPlus\Dotc.MQExplorerPlus.csproj /t:clean;rebuild /p:Configuration=Release 

set artifacts=%~dp0artifacts

pushd src\Dotc.MQExplorerPlus\bin\Release\net48

tar.exe -a -c -f %artifacts%\MQExplorerPlus%version%.zip *.*

popd






