$ErrorActionPreference = "Stop"

$publishTarget  = $PSScriptRoot + "\PublishOutput_Photo";

If(!(test-path $publishTarget))
{
      New-Item -ItemType Directory -Force -Path $publishTarget
}

dotnet publish .\PiDropPhoto\PiDropPhoto.csproj -r linux-arm -p:PublishSingleFile=true --self-contained true -c release -o "$publishTarget"