param(
	[string]$version = $null, 
	[string]$subversion = $null,
	[bool]$updatepackages = $false
)

function Output-Folder([string]$fullname = $null, [string]$name = $null) {

	$folder = split-path $fullname
	Write-Host $folder -foregroundcolor cyan -nonewline
	Write-Host " - " -nonewline
	Write-Host $name -foregroundcolor red
}

Write-Host "BUILD_BUILDNUMBER contents: $Env:BUILD_BUILDNUMBER"

#OK, if the version has not been passed in, then get it from the environment.
IF([string]::IsNullOrWhitespace($version)){$version=$Env:BUILD_BUILDNUMBER}
Write-Host "Assembly Version:" $version;

#Do we have a sub version for prerelease nuget packages?
IF(![string]::IsNullOrWhitespace($subversion)){$nugetversion=$version+"-"+$subversion}
Write-Host "Nuget Version:" $nugetversion;

#Set the Assembly versions for the DLLs.
(Get-Content Src\AssemblyInfo\SharedAssemblyInfo.cs).replace('AssemblyInformationalVersion("0.0.0.0")', 'AssemblyInformationalVersion("'+$nugetversion+'")') | Set-Content Src\AssemblyInfo\SharedAssemblyInfo.cs
(Get-Content Src\AssemblyInfo\SharedAssemblyInfo.cs).replace('0.0.0.0', $version) | Set-Content Src\AssemblyInfo\SharedAssemblyInfo.cs

#List out the NuSpec files in the solutuon
Get-ChildItem -Path "..\*\*.nuspec" -Recurse | ForEach-Object -Process {
	Output-Folder $_.fullname $_.name
}

#Update the NuSpec files with the correct version
Get-ChildItem -Path "..\*\*.nuspec" -Recurse | ForEach-Object -Process {
    (Get-Content $_) -Replace '{UpdateVersion.ps1.replace}', $nugetversion | Set-Content $_
}

Write-Host "All done here."
