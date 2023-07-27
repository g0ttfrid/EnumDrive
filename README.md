# EnumDrive
User enumeration - script to enumerate valid o365 users

### Instructions
```
PS C:\Users\IEUser> ipmo .\Invoke-EnumDrive.ps1

PS C:\Users\IEUser> Invoke-EnumDrive
[!] Arguments error
[!] Use for get tenant: Invoke-EnumDrive -method recon -domain domain.com.br
[!] Use for enum users: Invoke-EnumDrive -method enum -tenant tenant -file D:\users.txt

PS C:\Users\IEUser> Invoke-EnumDrive -method recon -domain domain.com.br
[>] OneDrive found: tenant

PS C:\Users\IEUser> Invoke-EnumDrive -method enum -tenant tenant -file D:\users.txt
[>] User found: tony.soprano@email.com
[>] User found: carmela.soprano@email.com
[>] User found: christopher.moltisanti@email.com
[>] User found: paulie.walnuts@email.com
[>] User found: silvio.dante@email.com
[>] User found: adriana.la.cerva@email.com
[>] User found: meadow.soprano@email.com
[>] User found: junior.soprano@email.com
[>] User found: bobby.bacala@email.com
[>] User found: janice.soprano@email.com
```

### Inspired by
[ONEDRIVE TO ENUM THEM ALL](https://www.trustedsec.com/blog/onedrive-to-enum-them-all/)<br>
