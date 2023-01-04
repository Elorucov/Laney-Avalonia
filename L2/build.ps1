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

echo "Current build: $($currentbuild)";

$location = "$(Get-Location)/bin/Release/net7.0";
if ($IsWindows) {
    $location = "$(Get-Location)\bin\Release\net7.0";
}

if ($IsWindows) {
    $btagw1 = "$($currentversion)-win-x64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagw1;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "publish --nologo -c Release -r win10-x64 --self-contained false -p:PublishSingleFile=true -p:Version=$($btagw1) -p:DefineConstants=WIN";
    echo "Win x86-64 is done.$([Environment]::NewLine)";

    $btagw3 = "$($currentversion)-win-arm64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagw3;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "publish --nologo -c Release -r win10-arm64 --self-contained false -p:PublishSingleFile=true -p:Version=$($btagw3) -p:DefineConstants=WIN";
    echo "Win arm64 is done.$([Environment]::NewLine)";
}

if ($IsLinux) {
	$btagl1 = "$($currentversion)-linux-x64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagl1;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "publish --nologo -c Release -r linux-x64 --self-contained false -p:PublishSingleFile=true -p:Version=$($btagl1) -p:DefineConstants=LINUX";
    echo "Linux x86-64 is done.$([Environment]::NewLine)";
}

if ($IsMacOS) {
    New-Item "$($location)/MacOS_Bundles/Laney_x86.app" -itemType Directory
    New-Item "$($location)/MacOS_Bundles/Laney_macOS11_arm64.app" -itemType Directory
    New-Item "$($location)/MacOS_Bundles/Laney_macOS12_arm64.app" -itemType Directory

    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "restore -r osx-x64";
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "restore -r osx.11.0-arm64";

    $btagm1 = "$($currentversion)-macos-x64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagm1;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "msbuild -t:BundleApp -property:Configuration=Release -p:RuntimeIdentifiers=osx-x64 -p:UseAppHost=true -p:Version=$($btagm1) -p:DefineConstants=MAC";
    Copy-Item "$($projfolder)/Assets/Logo/Laney.icns" -Destination "$($location)/publish/Laney.app/Contents/Resources";
    
    echo "Creating .app bundle file for macOS x86-64...";
    Copy-Item -Path "$($location)/publish/Laney.app/*" -Destination "$($location)/MacOS_Bundles/Laney_x86.app" -Recurse
    echo "Deleting publish folder...";
    Remove-Item -Path "$($location)/publish" -Recurse;
    echo "macOS x86-64 is done.$([Environment]::NewLine)";

    $btagm2 = "$($currentversion)-macos11-arm64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagm2;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "msbuild -t:BundleApp -property:Configuration=Release -p:RuntimeIdentifiers=osx.11.0-arm64 -p:UseAppHost=true -p:Version=$($btagm2) -p:DefineConstants=MAC";
    Copy-Item "$($projfolder)/Assets/Logo/Laney.icns" -Destination "$($location)/publish/Laney.app/Contents/Resources"
    
    echo "Creating .app bundle file for macOS 11 arm64...";
    Copy-Item -Path "$($location)/publish/Laney.app/*" -Destination "$($location)/MacOS_Bundles/Laney_macOS11_arm64.app" -Recurse
    echo "Deleting publish folder...";
    Remove-Item -Path "$($location)/publish" -Recurse;
    echo "macOS 11 arm64 is done.$([Environment]::NewLine)";

    $btagm2 = "$($currentversion)-macos-arm64-$([Environment]::UserName).$(hostname)-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";
    echo $btagm2;
    Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "msbuild -t:BundleApp -property:Configuration=Release -p:RuntimeIdentifiers=osx.12-arm64 -p:UseAppHost=true -p:Version=$($btagm2) -p:DefineConstants=MAC";
    Copy-Item "$($projfolder)/Assets/Logo/Laney.icns" -Destination "$($location)/publish/Laney.app/Contents/Resources"
        
    echo "Creating .app bundle file for macOS 12 arm64...";
    Copy-Item -Path "$($location)/publish/Laney.app/*" -Destination "$($location)/MacOS_Bundles/Laney_macOS12_arm64.app" -Recurse
    echo "Deleting publish folder...";
    Remove-Item -Path "$($location)/publish" -Recurse;
    echo "macOS 12 arm64 is done.$([Environment]::NewLine)";
}

Start-Process -NoNewWindow -Wait -FilePath 'dotnet' -ArgumentList "build-server shutdown";

echo "Next build: $($nextbuild). Saving build number in .csproj...";

$proj.Project.FirstChild.AssemblyVersion = $newversion;
$proj.Project.FirstChild.FileVersion = $newversion;
$proj.Project.PropertyGroup[1].CFBundleShortVersionString = $newversion;
$proj.Project.PropertyGroup[1].CFBundleVersion = $newversion;

# Unused in .csproj, но всё-таки пропишем, т. к. надо для dev-сборок и чтобы не поломался код проверки версии и даты в самом приложении
$proj.Project.FirstChild.Version = "$($newversion)-devplat-anycpu-devuser.devpc-$([DateTime]::Now.ToString("yyMMdd"))-$([DateTime]::UtcNow.ToString("HHmm"))";

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
