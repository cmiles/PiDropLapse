$ErrorActionPreference = "Stop"

$publishTarget  = $PSScriptRoot + "\PublishOutput_SimpleSensorReport";

If(!(test-path $publishTarget))
{
      New-Item -ItemType Directory -Force -Path $publishTarget
}

dotnet publish .\PiDropSimpleSensorReport\PiDropSimpleSensorReport.csproj -r linux-arm -p:PublishSingleFile=true --self-contained true -c release -o "$publishTarget"