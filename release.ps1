$version = "v100"

function pack([string]$path, [string]$name)
{
	cd $path
	del "appsettings.secret.json"
	del "*.pdb"
	del "*.log"
	& "C:\Program Files\7-Zip\7z.exe" a "..\..\..\..\..\..\GitsCommander-$($version)-$($name).zip"
	cd ..\..\..\..\..\..
}

dotnet publish
pack "GitsCommander\bin\Debug\net7.0\win-x64\publish" "windows-x64"


dotnet publish -r linux-x64 --self-contained false
pack "GitsCommander\bin\Debug\net7.0\linux-x64\publish" "linux-x64"
