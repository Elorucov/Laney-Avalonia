param (
    [string]$channel = ""
)
<# Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser #>

echo "=== L2 builder script by ELOR ===";

$edition = $PSVersionTable.PSEdition;
if ($edition -ne "Core") {
    echo "Please use PowerShell Core 6 and newer! Download it from here: https://aka.ms/powershell-release?tag=stable";
    exit;
}

$projfolder = Get-Location;
$file = "$($projfolder)/L2.csproj";
if ($IsWindows) {
    $file = "$($projfolder)\L2.csproj";
}
[xml]$proj = Get-Content -Path $($file);
$version = $proj.Project.FirstChild.AssemblyVersion;

$ver = $version.Split(".");
$currentbuild = [int]$ver[2];
$nextbuild = $currentbuild + 1;
$currentversion = "$($ver[0]).$($ver[1]).$($currentbuild)";
$newversion = "$($ver[0]).$($ver[1]).$($nextbuild)";
$chstr = "";
if ($channel -ne "") {
	$chstr = "%3B$($channel)";
	echo "Channel: $($channel)";
}

echo "Current build: $($currentbuild)";

$location = "$(Get-Location)/bin/Release/net7.0";
if ($IsWindows) {
    $location = "$(Get-Location)\bin\Release\net7.0";
}

if ($IsWindows) {
    $btagw1 = "$($currentversion)-win-x64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagw1;
    $proc1 = Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -PassThru -ArgumentList "publish --nologo -c Release -r win10-x64 -p:EnableCompressionInSingleFile=true --p:PublishAOT=true -p:ServerGarbageCollection=true -p:PublishReadyToRun=true -p:Version=$($btagw1) -p:DefineConstants=WIN$($chstr)";
    $proc1.WaitForExit();
	echo "Win x86-64 is done.$([Environment]::NewLine)";

    $btagw3 = "$($currentversion)-win-arm64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagw3;
    $proc2 = Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -PassThru -ArgumentList "publish --nologo -c Release -r win10-arm64 -p:EnableCompressionInSingleFile=true --p:PublishAOT=true -p:ServerGarbageCollection=true -p:PublishReadyToRun=true -p:Version=$($btagw3) -p:DefineConstants=WIN$($chstr)";
    $proc2.WaitForExit();
	echo "Win arm64 is done.$([Environment]::NewLine)";
}

if ($IsLinux) {
	$btagl1 = "$($currentversion)-linux-x64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagl1;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "publish --nologo -c Release -r linux-x64 -p:EnableCompressionInSingleFile=true -p:ServerGarbageCollection=true -p:PublishAOT=true -p:PublishReadyToRun=true -p:Version=$($btagl1) -p:DefineConstants=LINUX$($chstr)";
    echo "Linux x86-64 is done.$([Environment]::NewLine)";
}

if ($IsMacOS) {
    New-Item "$($location)/MacOS_Bundles/x64/Laney.app" -itemType Directory
    New-Item "$($location)/MacOS_Bundles/arm64/Laney.app" -itemType Directory

    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "restore -r osx-x64";
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "restore -r osx-arm64";

    $btagm1 = "$($currentversion)-macos-x64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagm1;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "msbuild -t:BundleApp -property:Configuration=Release -p:RuntimeIdentifiers=osx-x64 -p:UseAppHost=true -p:Version=$($btagm1) -p:DefineConstants=MAC$($chstr)";
    Copy-Item "$($projfolder)/Assets/Logo/icon.icns" -Destination "$($location)/publish/Laney.app/Contents/Resources";
    
    echo "Creating .app bundle file for macOS x86-64...";
    Copy-Item -Path "$($location)/publish/Laney.app/*" -Destination "$($location)/MacOS_Bundles/x64/Laney.app" -Recurse
    echo "Deleting publish folder...";
    Remove-Item -Path "$($location)/publish" -Recurse;
    echo "macOS x86-64 is done.$([Environment]::NewLine)";

    $btagm2 = "$($currentversion)-macos-arm64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagm2;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "msbuild -t:BundleApp -property:Configuration=Release -p:RuntimeIdentifiers=osx-arm64 -p:UseAppHost=true -p:Version=$($btagm2) -p:DefineConstants=MAC$($chstr)";
    Copy-Item "$($projfolder)/Assets/Logo/icon.icns" -Destination "$($location)/publish/Laney.app/Contents/Resources"
    
    echo "Creating .app bundle file for macOS arm64...";
    Copy-Item -Path "$($location)/publish/Laney.app/*" -Destination "$($location)/MacOS_Bundles/arm64/Laney.app" -Recurse
    echo "Deleting publish folder...";
    Remove-Item -Path "$($location)/publish" -Recurse;
    echo "macOS 11 arm64 is done.$([Environment]::NewLine)";
}

Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "build-server shutdown";

echo "Next build: $($nextbuild). Saving build number in .csproj...";

$proj.Project.FirstChild.AssemblyVersion = $newversion;
$proj.Project.FirstChild.FileVersion = $newversion;
$proj.Project.PropertyGroup[1].CFBundleShortVersionString = $newversion;
$proj.Project.PropertyGroup[1].CFBundleVersion = $newversion;

# Unused in .csproj, но всё-таки пропишем, т. к. надо для dev-сборок и чтобы не поломался код проверки версии и даты в самом приложении
$proj.Project.FirstChild.Version[0].InnerXML = "$($newversion)-win-anycpu-devuser.devpc-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
$proj.Project.FirstChild.Version[1].InnerXML = "$($newversion)-linux-anycpu-devuser.devpc-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
$proj.Project.FirstChild.Version[2].InnerXML = "$($newversion)-macos-anycpu-devuser.devpc-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";

#Settings object will instruct how the xml elements are written to the file
$settings = New-Object System.Xml.XmlWriterSettings
$settings.Indent = $true
#NewLineChars will affect all newlines
$settings.NewLineChars ="`r`n"
#Set an optional encoding, UTF-8 is the most used (without BOM)
$settings.Encoding = New-Object System.Text.UTF8Encoding($false)

$w = [System.Xml.XmlWriter]::Create($file, $settings);
$proj.Save($w);
$w.Close();

if ($IsWindows) {
    echo "All done! Check $($location)\win-<arch>\publish folder.";
}
if (!$IsWindows) {
    echo "All done! Check $($location)/<platform>/publish folder. For macOS, check $($location)/MacOS_Bundles folder.";
}
