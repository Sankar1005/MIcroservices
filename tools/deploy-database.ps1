<#
.SYNOPSIS
    Builds database/Microservices.Database and publishes it to a SQL Server database.

.DESCRIPTION
    SqlPackage compares the .dacpac with the target database and runs only the changes needed.
    By default it refuses changes that would lose data (e.g. dropping a column that has values).

.EXAMPLE
    .\tools\deploy-database.ps1                 # publish to (localdb)\MSSQLLocalDB, database Microservices
    .\tools\deploy-database.ps1 -ReportOnly     # only show what would change, change nothing
#>
param(
    [string] $Server = '(localdb)\MSSQLLocalDB',
    [string] $Database = 'Microservices',
    [switch] $ReportOnly
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'database\Microservices.Database\Microservices.Database.sqlproj'
$dacpac = Join-Path $root 'database\Microservices.Database\bin\Release\Microservices.Database.dacpac'
# Windows Authentication only: no passwords in scripts or in the repository.
$connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True"

Push-Location $root
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed.' }

    dotnet build $project -c Release -nologo
    if ($LASTEXITCODE -ne 0) { throw 'Building the database project failed.' }

    if ($ReportOnly) {
        $report = Join-Path $root 'database\Microservices.Database\bin\Release\DeployReport.xml'
        dotnet sqlpackage /Action:DeployReport /SourceFile:$dacpac /TargetConnectionString:$connectionString /OutputPath:$report /OverwriteFiles:True
        if ($LASTEXITCODE -ne 0) { throw 'DeployReport failed.' }
        Get-Content $report
    }
    else {
        dotnet sqlpackage /Action:Publish /SourceFile:$dacpac /TargetConnectionString:$connectionString
        if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
    }
}
finally {
    Pop-Location
}
