--set msBuildDir="%ProgramFiles(X86)%\MSBuild\14.0\bin"
set vswhere="%ProgramFiles(X86)%\Microsoft Visual Studio\Installer\vswhere.exe"

for /f "usebackq tokens=1* delims=: " %%i in (`%vswhere% -latest -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" set InstallDir=%%j
)

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (

  .nuget\nuget.exe restore src\MQExplorerPlus.sln
  
  mkdir artifacts
  
  "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" src\Dotc.MQExplorerPlus.Setup\Dotc.MQExplorerPlus.Setup.wixproj /t:Rebuild  /p:Configuration=Release /l:FileLogger,Microsoft.Build.Engine;logfile=artifacts\Manual_MSBuild_MSI_LOG.log
  
)





