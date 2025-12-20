# Define the array of paths (relative or absolute)
$paths = @(
    "..\src\Host",
    "..\tests\Modules\Shared.Users.Tests.EndToEnd"
)

# Iterate through each path
foreach ($path in $paths) {
    
    # Check if the directory exists
    if (Test-Path $path) {
        Write-Host "--------------------------------------------------"
        Write-Host "Processing directory: $path" -ForegroundColor Cyan
        
        # Change directory to the target path (saving the current location to the stack)
        Push-Location -Path $path
        
        try {
            # Execute the dotnet command
            Write-Host "Running: dotnet tool run shared env generate -f" -ForegroundColor DarkGray
            dotnet tool run shared env generate -f
        }
        catch {
            Write-Error "An error occurred while executing the command in: $path"
        }
        finally {
            # Return to the original directory so the next relative path works correctly
            Pop-Location
        }
    }
    else {
        Write-Warning "Path does not exist: $path"
    }
}

Write-Host "--------------------------------------------------"
Write-Host "Finished." -ForegroundColor Green