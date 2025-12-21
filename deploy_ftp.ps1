param(
    [string]$LocalPath = ".\publish_self_contained",
    [string]$FtpUrl = "ftp://win1015.site4now.net:21/nova",
    [string]$Username = "dummystate3-001",
    [string]$Password = "dummyaccount3"
)

# Ensure local path is absolute
$LocalPath = (Resolve-Path $LocalPath).Path
Write-Host "Deploying from: $LocalPath"
Write-Host "Target URL: $FtpUrl"

# Cache created directories to avoid redundant requests
$global:createdDirs = @{}

function Get-FtpRequest {
    param($Url, $Method)
    $req = [System.Net.FtpWebRequest]::Create($Url)
    $req.Method = $Method
    $req.Credentials = New-Object System.Net.NetworkCredential($Username, $Password)
    $req.UseBinary = $true
    $req.KeepAlive = $false
    $req.UsePassive = $true
    return $req
}

function Create-RemoteDirectory {
    param($BaseUrl, $RelativePath)
    
    if ([string]::IsNullOrEmpty($RelativePath) -or $RelativePath -eq ".") { return }
    
    # Normalize segments
    $segments = $RelativePath -split "/"
    $currentPath = ""
    
    foreach ($segment in $segments) {
        if ([string]::IsNullOrEmpty($segment)) { continue }
        
        if ($currentPath -eq "") {
            $currentPath = $segment
        } else {
            $currentPath = "$currentPath/$segment"
        }
        
        if ($global:createdDirs.ContainsKey($currentPath)) { continue }
        
        $dirUrl = "$BaseUrl/$currentPath"
        try {
            # Try to create
            $req = Get-FtpRequest -Url $dirUrl -Method ([System.Net.WebRequestMethods+Ftp]::MakeDirectory)
            $resp = $req.GetResponse()
            $resp.Close()
            Write-Host "Created directory: $currentPath" -ForegroundColor Cyan
            $global:createdDirs[$currentPath] = $true
        } catch {
            # 550 means directory exists or cannot be created. Assume exists.
            $global:createdDirs[$currentPath] = $true
        }
    }
}

function Upload-File {
    param($LocalFile, $RemoteFileUrl)
    
    Write-Host "Uploading $LocalFile to $RemoteFileUrl"
    
    try {
        $req = Get-FtpRequest -Url $RemoteFileUrl -Method ([System.Net.WebRequestMethods+Ftp]::UploadFile)
        $fileContent = [System.IO.File]::ReadAllBytes($LocalFile)
        $req.ContentLength = $fileContent.Length

        $stream = $req.GetRequestStream()
        $stream.Write($fileContent, 0, $fileContent.Length)
        $stream.Close()

        $resp = $req.GetResponse()
        $resp.Close()
        # Write-Host "  Success" -ForegroundColor Green
    } catch {
        Write-Host "  FAILED to upload $LocalFile" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  Full Error: $_" -ForegroundColor Red
        # throw # Uncomment to stop on first error
    }
}

# 1. Get all files
$files = Get-ChildItem -Path $LocalPath -Recurse -File

# 2. Process
foreach ($file in $files) {
    # Robust relative path calculation
    # Robust relative path calculation
    $rootPath = $LocalPath
    if (-not $rootPath.EndsWith("\")) { $rootPath += "\" }
    
    $relativePath = $file.FullName.Replace($rootPath, "").Replace("\", "/")
    
    if ([string]::IsNullOrEmpty($relativePath)) {
        Write-Warning "Skipping $($file.FullName) - Relative path empty?"
        continue
    }
    
    $remoteFileUrl = "$FtpUrl/$relativePath"
    
    # Ensure directory exists
    $dirName = [System.IO.Path]::GetDirectoryName($relativePath).Replace("\", "/")
    Create-RemoteDirectory -BaseUrl $FtpUrl -RelativePath $dirName
    
    
    # Upload
    Upload-File -LocalFile $file.FullName -RemoteFileUrl $remoteFileUrl
    
    # Be gentle on the server
    Start-Sleep -Milliseconds 200
}

Write-Host "Deployment complete!" -ForegroundColor Green
