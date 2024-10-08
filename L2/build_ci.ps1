param (
    [string]$appname = "",
    [string]$channel = "",
    [string]$output = "",
    [string]$ctarget = "",
    [string]$repoowner = "",
    [string]$reponame = ""
)
<# Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser #>

if ($appname -ne "") {
	echo "=== $($appname) builder script by ELOR ===";
} else {
    echo "Required appname argument! (example: build.ps1 -appname HelloWorld). Note: this script can use only for CI/CD!";
    exit;
}

$edition = $PSVersionTable.PSEdition;
if ($edition -ne "Core") {
    echo "Please use PowerShell Core 6 and newer! Download it from here: https://aka.ms/powershell-release?tag=stable";
    exit;
}

$projfolder = "$(Get-Location)/L2";
echo "Proj folder: $($projfolder)";
$file = "$($projfolder)/L2.csproj";
[xml]$proj = Get-Content -Path $($file);
$version = $proj.Project.FirstChild.AssemblyVersion;

$ver = $version.Split(".");
$currentbuild = [int]$ver[2];
$currentversion = "$($ver[0]).$($ver[1]).$($currentbuild)";

$chstr = "";
if ($channel -ne "") {
	$chstr = "%3B$($channel)";
	echo "Channel: $($channel)";
}

$outstr = "";
if ($output -ne "") {
    $outstr = '-o "{0}"' -f $output;
	echo "Output folder: $($output)";
    echo $outstr;
}

if ($ctarget -ne "") {
    echo "Target: $($ctarget)";
} else {
    echo "Required ctarget argument! (example: build.ps1 -ctarget win-x64)";
    exit;
}

echo "Current build: $($currentbuild)";

$location = $output;

$uname = [Environment]::UserName;
$hname = "$(hostname)";
$uhnamefull = [System.Text.Encoding]::UTF8.GetBytes("$($uname).$($hname)");
$uhnamenc = [System.Convert]::ToBase64String($uhnamefull).Replace("=", ".4444");

$repofull = [System.Text.Encoding]::UTF8.GetBytes("$($reponame)");
$repoenc = [System.Convert]::ToBase64String($repofull).Replace("=", ".4444");

if ($IsWindows) {
    $const = "WIN$($chstr)";
    $btagw1 = "$($currentversion)-$($ctarget)-$($uhnamenc)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))-$($repoenc)";
    echo $btagw1;
    dotnet publish --nologo -c Release -r $ctarget -o $output -p:EnableCompressionInSingleFile=true -p:PublishAOT=true -p:OptimizationPreference=Size -p:StackTraceSupport=false -p:UseSystemResourceKeys=true -p:DebugType=None -p:DebugSymbols=False -p:DebuggerSupport=false -p:Version=$btagw1 -p:DefineConstants=$const;
    echo "$($appname) $($ctarget) is done.$([Environment]::NewLine)";
}

if ($IsLinux) {
    $const = "LINUX$($chstr)";
    $btagl1 = "$($currentversion)-$($ctarget)-$($uhnamenc)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))-$($repoenc)";
    echo $btagl1;
    dotnet publish --nologo -c Release -r $ctarget -o $output -p:EnableCompressionInSingleFile=true -p:PublishAOT=true -p:OptimizationPreference=Size -p:StackTraceSupport=false -p:UseSystemResourceKeys=true -p:DebugType=None -p:DebugSymbols=False -p:DebuggerSupport=false -p:Version=$btagl1 -p:DefineConstants=$const;
    echo "$($appname) $($ctarget) is done.$([Environment]::NewLine)";
}

if ($IsMacOS) {
    $const = "MAC$($chstr)";

    dotnet restore -r $($ctarget);
    $mlocation = "$($projfolder)/bin/Release/net8.0";

    $btagm1 = "$($currentversion)-$($ctarget)-$($uname).$($hname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagm1;
    dotnet publish --nologo -c Release -r $ctarget -o $output -p:EnableCompressionInSingleFile=true -p:PublishAOT=true -p:OptimizationPreference=Size -p:StackTraceSupport=false -p:UseSystemResourceKeys=true -p:DebugType=None -p:DebugSymbols=False -p:DebuggerSupport=false -p:Version=$btagm1 -p:DefineConstants=$const;
    
    echo "Creating .app bundle file for macOS...";
    Copy-Item -Path "$($output)/*" -Destination "$(Get-Location)/MacOS_layout/Contents/MacOS" -Recurse;
    Remove-Item -Path "$(Get-Location)/MacOS_layout/Contents/MacOS/empty";
    Get-ChildItem -LiteralPath "$(Get-Location)/MacOS_layout/Contents/MacOS" -Filter *.dsym | Remove-Item -Force -Recurse
    Copy-Item -Path "$(Get-Location)/MacOS_layout/*" -Destination "$($location)/MacOS_Bundles/$($appname).app/Contents" -Recurse;
    echo "$($appname) $($ctarget) is done.$([Environment]::NewLine)";
}

dotnet build-server shutdown;