# 一键编译：pwsh -ExecutionPolicy Bypass -File .\build.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src  = Join-Path $root 'src'
$dist = Join-Path $root 'dist'
New-Item -ItemType Directory -Force -Path $dist | Out-Null

$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) { $csc = "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" }

$out = Join-Path $dist 'NetBlock.exe'
& $csc /nologo /target:winexe /platform:anycpu /optimize+ /codepage:65001 `
      "/out:$out" `
      "/win32icon:$(Join-Path $src 'app.ico')" `
      "/win32manifest:$(Join-Path $src 'app.manifest')" `
      /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
      (Join-Path $src 'NetBlock.cs')

if (-not (Test-Path $out)) { throw '编译失败' }
$fi = Get-Item $out
"OK  ->  $($fi.FullName)   $([math]::Round($fi.Length/1KB,1)) KB"
