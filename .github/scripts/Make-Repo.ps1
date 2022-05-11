$ErrorActionPreference = 'SilentlyContinue'

$output = New-Object Collections.Generic.List[object]

$content = Get-Content "build/BlindBoxPlugin.json" | ConvertFrom-Json

$dlTemplate = "https://github.com/he0119/FFXIVBlindBoxPlugin/releases/download/v{0}/latest.zip"

$content | add-member -Name "IsHide" -value "False" -MemberType NoteProperty

$content | add-member -Name "IsTestingExclusive" -value "False" -MemberType NoteProperty

$dlCount = 0;
$content | add-member -Name "DownloadCount" $dlCount -MemberType NoteProperty

$updateDate = Get-Date -UFormat %s
$content | add-member -Name "LastUpdate" $updateDate -MemberType NoteProperty

$assemblyVersion = $content.AssemblyVersion
$dlLink = $dlTemplate -f $assemblyVersion

$content | add-member -Name "DownloadLinkInstall" $dlLink -MemberType NoteProperty
$content | add-member -Name "DownloadLinkTesting" $dlLink -MemberType NoteProperty
$content | add-member -Name "DownloadLinkUpdate" $dlLink -MemberType NoteProperty

$output.Add($content)

# https://stackoverflow.com/questions/18662967/convertto-json-an-array-with-a-single-item
$outputStr = ConvertTo-Json $output
Write-Output $outputStr

Out-File -FilePath .\repo.json -InputObject $outputStr
