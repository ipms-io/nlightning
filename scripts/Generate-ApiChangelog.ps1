# API Changes Analysis
param(
    [string]$FromRef = "HEAD~20",
    [string]$ToRef = "HEAD",
    [string]$ProjectPath = "src/NLightning.Bolt11"
)

Write-Host "# NLightning.Bolt11 API Changes" -ForegroundColor Green
Write-Host "From $FromRef to $ToRef" -ForegroundColor Gray
Write-Host "="*50 -ForegroundColor Gray

# Get all changed C# files in the Bolt11 project
$changedFiles = git diff --name-only "$FromRef..$ToRef" -- "$ProjectPath/**/*.cs" | Where-Object { $_ -match '\.cs$' }

if (-not $changedFiles)
{
    Write-Host "No C# files changed in $ProjectPath" -ForegroundColor Yellow
    exit
}

Write-Host "`n## Changed Files:" -ForegroundColor Cyan
$changedFiles | ForEach-Object { Write-Host "- $_" -ForegroundColor White }

$apiChanges = @{
    "New Classes/Interfaces" = @()
    "Modified Classes" = @()
    "New Public Methods" = @()
    "Modified Public Methods" = @()
    "New Properties" = @()
    "Breaking Changes" = @()
    "New Namespaces" = @()
}

foreach ($file in $changedFiles)
{
    if (!(Test-Path $file))
    {
        $apiChanges["Breaking Changes"] += "❌ **Deleted**: $file"
        continue
    }

    Write-Host "`n### Analyzing: $file" -ForegroundColor Yellow

    # Get the diff for this file
    $diff = git diff "$FromRef..$ToRef" -- $file

    if (-not $diff)
    {
        continue
    }

    $diffLines = $diff -split "`n"
    $inClass = $false
    $currentClass = ""

    foreach ($line in $diffLines)
    {
        $cleanLine = $line.Trim()

        # Skip context lines and focus on additions/deletions
        if (-not ($cleanLine.StartsWith('+') -or $cleanLine.StartsWith('-')))
        {
            continue
        }

        $isAddition = $cleanLine.StartsWith('+')
        $isDeletion = $cleanLine.StartsWith('-')
        $content = $cleanLine.Substring(1).Trim()

        # Skip empty lines and comments
        if (-not $content -or $content.StartsWith('//') -or $content.StartsWith('/*') -or $content.StartsWith('*'))
        {
            continue
        }

        # New/Modified namespace
        if ($content -match '^namespace\s+([^\s;{]+)')
        {
            $namespace = $matches[1]
            if ($isAddition)
            {
                $apiChanges["New Namespaces"] += " **New Namespace**: $namespace in $file"
            }
        }

        # New/Modified public class, interface, enum, struct
        if ($content -match '(?:public|internal)\s+(?:static\s+)?(?:partial\s+)?(class|interface|enum|struct|record)\s+([^\s<:{]+)')
        {
            $type = $matches[1]
            $name = $matches[2]

            if ($isAddition)
            {
                $apiChanges["New Classes/Interfaces"] += "✨ **New $type**: $name in $file"
            }
            elseif ($isDeletion)
            {
                $apiChanges["Breaking Changes"] += "❌ **Deleted $type**: $name in $file"
            }
            $currentClass = $name
        }

        # New/Modified public methods
        if ($content -match '(?:public|internal)\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?(?:async\s+)?[\w\[\]<>?,\s]+\s+([^\s(]+)\s*\([^)]*\)')
        {
            $methodName = $matches[1]

            # Skip properties, constructors, and operators
            if ($methodName -match '^(get|set|add|remove)$' -or $methodName -eq $currentClass -or $methodName.StartsWith('operator'))
            {
                continue
            }

            if ($isAddition)
            {
                $apiChanges["New Public Methods"] += " **New Method**: $methodName in $currentClass ($file)"
            }
            elseif ($isDeletion)
            {
                $apiChanges["Breaking Changes"] += " **Removed Method**: $methodName in $currentClass ($file)"
            }
        }

        # New/Modified public properties
        if ($content -match '(?:public|internal)\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?[\w\[\]<>?,\s]+\s+([^\s{(]+)\s*\{\s*(?:get|set)')
        {
            $propName = $matches[1]

            if ($isAddition)
            {
                $apiChanges["New Properties"] += "️ **New Property**: $propName in $currentClass ($file)"
            }
            elseif ($isDeletion)
            {
                $apiChanges["Breaking Changes"] += " **Removed Property**: $propName in $currentClass ($file)"
            }
        }

        # Check for signature changes (both addition and deletion of same method name)
        if (($isAddition -or $isDeletion) -and $content -match '(?:public|internal)\s+.*?\s+([^\s(]+)\s*\(')
        {
            $methodName = $matches[1]
            # This is a simplified check - in reality, you'd want to track pairs of additions/deletions
            if ($isDeletion)
            {
                $apiChanges["Modified Public Methods"] += "⚠️ **Modified Method Signature**: $methodName in $currentClass ($file)"
            }
        }
    }
}

# Display results
Write-Host "`n##  API Changes Summary:" -ForegroundColor Green

foreach ($category in $apiChanges.Keys)
{
    if ($apiChanges[$category].Count -gt 0)
    {
        Write-Host "`n### $category" -ForegroundColor Cyan
        $apiChanges[$category] | ForEach-Object {
            Write-Host "  $_" -ForegroundColor White
        }
    }
}

# Get file statistics
Write-Host "`n##  Change Statistics:" -ForegroundColor Green
$stats = git diff --stat "$FromRef..$ToRef" -- "$ProjectPath/**/*.cs"
if ($stats)
{
    $stats | ForEach-Object { Write-Host $_ -ForegroundColor White }
}

# Check for version changes in project file
$projectFile = "$projectPath/*.csproj"
if (Test-Path $projectFile)
{
    $versionDiff = git diff "$FromRef..$ToRef" -- $projectFile | Select-String -Pattern "Version"
    if ($versionDiff)
    {
        Write-Host "`n## ️ Version Changes:" -ForegroundColor Green
        $versionDiff | ForEach-Object { Write-Host $_.Line.Trim() -ForegroundColor White }
    }
}

Write-Host "`n✅ Analysis complete!" -ForegroundColor Green