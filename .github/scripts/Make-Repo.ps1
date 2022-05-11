$ErrorActionPreference = 'SilentlyContinue'

$output = New-Object Collections.Generic.List[object]

$content = Get-Content "build/BlindBoxPlugin.json" | ConvertFrom-Json

$dlTemplateInstall = "https://github.com/he0119/FFXIVBlindBoxPlugin/releases/download/v{0}/latest.zip"
$dlTemplateUpdate = "https://github.com/he0119/FFXIVBlindBoxPlugin/releases/download/v{0}/latest.zip"

$content | add-member -Name "IsHide" -value "False" -MemberType NoteProperty

$content | add-member -Name "IsTestingExclusive" -value "False" -MemberType NoteProperty

$dlCount = 0;
$content | add-member -Name "DownloadCount" $dlCount -MemberType NoteProperty

$updateDate = Get-Date -UFormat %s
$content | add-member -Name "LastUpdate" $updateDate -MemberType NoteProperty

$assemblyVersion = $content.AssemblyVersion
$installLink = $dlTemplateInstall -f $assemblyVersion
$content | add-member -Name "DownloadLinkTesting" $installLink -MemberType NoteProperty

$updateLink = $dlTemplateUpdate -f $assemblyVersion
$content | add-member -Name "DownloadLinkUpdate" $updateLink -MemberType NoteProperty

$output.Add($content)

# https://stackoverflow.com/questions/18662967/convertto-json-an-array-with-a-single-item
$outputStr = ConvertTo-Json $output
Write-Output $outputStr

Out-File -FilePath .\repo.json -InputObject $outputStr
