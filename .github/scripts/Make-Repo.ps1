$ErrorActionPreference = 'SilentlyContinue'

$output = New-Object Collections.Generic.List[object]
$notInclude = "MapLinker";

$counts = Get-Content "downloadcounts.json" | ConvertFrom-Json

$dlTemplateInstall = "https://github.com/he0119/DalamudPlugins/raw/main/plugins/{0}/latest.zip"
$dlTemplateUpdate = "https://github.com/he0119/DalamudPlugins/raw/main/plugins/{0}/latest.zip"

$thisPath = Get-Location

$table = ""

$content = Get-Content "build/BlindBoxPlugin/BlindBoxPlugin.json" | ConvertFrom-Json

if ($notInclude.Contains($content.InternalName)) {
    $content | add-member -Name "IsHide" -value "True" -MemberType NoteProperty
}
else
{
    $content | add-member -Name "IsHide" -value "False" -MemberType NoteProperty

    $newDesc = $content.Description -replace "\n", "<br>"
    $table = $table + "| " + $content.Author + " | " + $content.Name + " | " + $newDesc + " |`n"
}

$testingPath = Join-Path $thisPath -ChildPath "testing" | Join-Path -ChildPath $content.InternalName | Join-Path -ChildPath $_.Name
if ($testingPath | Test-Path)
{
    $testingContent = Get-Content $testingPath | ConvertFrom-Json
    $content | add-member -Name "TestingAssemblyVersion" -value $testingContent.AssemblyVersion -MemberType NoteProperty
}
$content | add-member -Name "IsTestingExclusive" -value "False" -MemberType NoteProperty

$dlCount = $counts | Select-Object -ExpandProperty $content.InternalName | Select-Object -ExpandProperty "count"
if ($dlCount -eq $null){
    $dlCount = 0;
}
$content | add-member -Name "DownloadCount" $dlCount -MemberType NoteProperty

$internalName = $content.InternalName
$updateDate = git log -1 --pretty="format:%ct" plugins/$internalName/latest.zip
$content | add-member -Name "LastUpdate" $updateDate -MemberType NoteProperty

$installLink = $dlTemplateInstall -f $internalName, "False"
$content | add-member -Name "DownloadLinkInstall" $installLink -MemberType NoteProperty

$installLink = $dlTemplateInstall -f $internalName, "True"
$content | add-member -Name "DownloadLinkTesting" $installLink -MemberType NoteProperty

$updateLink = $dlTemplateUpdate -f $internalName, "False"
$content | add-member -Name "DownloadLinkUpdate" $updateLink -MemberType NoteProperty

$output.Add($content)

# https://stackoverflow.com/questions/18662967/convertto-json-an-array-with-a-single-item
$outputStr = ConvertTo-Json $output
Write-Output $outputStr

Out-File -FilePath .\repo.json -InputObject $outputStr
