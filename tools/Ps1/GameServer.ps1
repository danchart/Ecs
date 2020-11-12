

$Body = @{
     User = 'jdoe'
     password = 'P@S$w0rd!'
 } |ConvertTo-Json

 Invoke-WebRequest -Uri "http://localhost:8110/player/123/connect" -Method Post -ContentType "application/json" -Body $Body