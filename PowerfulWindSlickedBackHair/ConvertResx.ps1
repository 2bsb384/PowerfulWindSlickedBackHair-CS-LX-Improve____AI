# ConvertResx.ps1
# 将项目内所有 .resx 预编译为经典格式 .resources（.NET Framework ResourceWriter），
# 输出到 obj\ResGen\，文件名即最终嵌入的逻辑资源名。
# 供没有 Visual Studio 的环境（dotnet msbuild 的 GenerateResource 会拒收非字符串 resx）使用。
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms

$projectDir = $PSScriptRoot
$outDir = Join-Path $projectDir "obj\ResGen"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$resxFiles = @()
$resxFiles += Get-ChildItem -LiteralPath $projectDir -Filter *.resx
$resxFiles += Get-ChildItem -LiteralPath (Join-Path $projectDir "Properties") -Filter *.resx
$winDir = Join-Path $projectDir "Windows"
if (Test-Path $winDir) { $resxFiles += Get-ChildItem -LiteralPath $winDir -Filter *.resx }

foreach ($resx in $resxFiles) {
    $relDir = $resx.DirectoryName.Substring($projectDir.Length).TrimStart('\', '/').Replace('\', '.').Replace('/', '.')
    $base = [System.IO.Path]::GetFileNameWithoutExtension($resx.Name)
    $prefix = "PowerfulWindSlickedBackHair."
    if ($relDir) { $prefix += $relDir + "." }
    $logical = $prefix + $base + ".resources"
    $outFile = Join-Path $outDir $logical

    $reader = New-Object System.Resources.ResXResourceReader($resx.FullName)
    $reader.BasePath = $resx.DirectoryName

    $writer = New-Object System.Resources.ResourceWriter($outFile)
    foreach ($entry in $reader) {
        $name = [string]$entry.Key
        $val = $entry.Value
        if ($null -eq $val) {
            Write-Warning "跳过空条目: $logical : $name"
            continue
        }
        if ($val -is [System.Resources.ResXFileRef]) {
            $path = $val.TextFile
            if (-not [System.IO.Path]::IsPathRooted($path)) {
                $path = [System.IO.Path]::GetFullPath((Join-Path $reader.BasePath $path))
            }
            $t = [System.Type]::GetType($val.TypeName, $true)
            $conv = [System.ComponentModel.TypeDescriptor]::GetConverter($t)
            $obj = $conv.ConvertFromInvariantString($path)
            $writer.AddResource($name, [object]$obj)
        }
        elseif ($val -is [string]) {
            $writer.AddResource($name, [string]$val)
        }
        else {
            $writer.AddResource($name, [object]$val)
        }
    }
    $writer.Generate()
    $writer.Close()
    $reader.Close()
    Write-Host "GEN  $logical"
}
Write-Host "DONE -> $outDir"