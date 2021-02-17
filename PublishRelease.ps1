$ErrorActionPreference = "Stop"

$publishTarget  = $PSScriptRoot + "\PublishOutput";

If(!(test-path $publishTarget))
{
      New-Item -ItemType Directory -Force -Path $publishTarget
}

dotnet publish -r linux-arm -p:PublishSingleFile=true --self-contained true -c release -o "$publishTarget"