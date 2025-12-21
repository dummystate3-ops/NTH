
param(
    [string]$FtpUrl = "ftp://win1015.site4now.net:21/nova",
    [string]$Username = "dummystate3-001",
    [string]$Password = "dummyaccount3"
)

function Delete-File {
    param($Url)
    Write-Host "Deleting $Url"
    try {
        $req = [System.Net.FtpWebRequest]::Create($Url)
        $req.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
        $req.Credentials = New-Object System.Net.NetworkCredential($Username, $Password)
        $req.KeepAlive = $false
        
        $resp = $req.GetResponse()
        Write-Host "Success! Deleted." -ForegroundColor Green
        $resp.Close()
    } catch {
        Write-Host "Failed Delete: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Delete-File -Url "$FtpUrl/default.asp"
