function Invoke-EnumDrive
{
    <#
    .DESCRIPTION
        
        Enumerate valid users via onedrive.
        Author: g0ttfrid

    .EXAMPLE
        
        PS C:\> Invoke-EnumDrive -method recon -domain domain.com.br

        Search tenant.

    .EXAMPLE

        PS C:\> Invoke-EnumDrive -method enum -tenant tenant -file D:\users.txt

        Enumerate valid users.
    #>

    [CmdletBinding()]
    Param (
        [Parameter(Mandatory=$false)][string]$method,
        [Parameter(Mandatory=$false)][string]$domain,
        [Parameter(Mandatory=$false)][string]$tenant,
        [Parameter(Mandatory=$false)][string]$file
    )
    
    $a=New-Object IO.MemoryStream(,[Convert]::FromBAsE64String("H"+"4sI"+"AA"+"AA"+"AAA"+"EAO"+"07"+"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"+"fEbA"+"EI"+"AA"+"A="+"="))
    $b = New-Object IO.Compression.GzipStream($a,[IO.Compression.CoMPressionMode]::DEComPress)
    $c = New-Object System.IO.MemoryStream
    $b.CopyTo( $c )
    [byte[]] $d = $c.ToArray()
    $assembly = [System.Reflection.Assembly]::Load($d)

    if (-not [string]::IsNullOrEmpty($domain) -and $method -match "recon")
    {
        [EnumDrive.Program]::Main(@($method, $domain)) | Out-Null
    }
    elseif (-not [string]::IsNullOrEmpty($tenant) -and -not [string]::IsNullOrEmpty($file) -and $method -match "enum")
    {
        $resultTask = [EnumDrive.Program]::Main(@($method, $tenant, $file))
        $resultTask.GetAwaiter().GetResult() | Out-Null
    }
    else
    {
        Write-Host "[!] Arguments error"
        Write-Host "[!] Use for get tenant: Invoke-EnumDrive -method recon -domain domain.com.br"
        Write-Host "[!] Use for enum users: Invoke-EnumDrive -method enum -tenant tenant -file D:\users.txt"
    }
}
