$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'Requires .NET Framework C# compiler on Windows.' }
$testOutput = Join-Path $env:TEMP ('ThermalTwinTests-' + [guid]::NewGuid().ToString('N') + '.exe')
& $compiler /nologo /target:exe "/out:$testOutput" (Join-Path $projectRoot 'Assets/Scripts/ThermalField.cs') (Join-Path $projectRoot 'Assets/Scripts/ChamberLayoutRules.cs') (Join-Path $PSScriptRoot 'CoreTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Core compilation failed.' }
& $testOutput
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }
Remove-Item -LiteralPath $testOutput
