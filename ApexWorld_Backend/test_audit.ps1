$tokenResponse = Invoke-RestMethod -Uri "http://localhost:5029/api/v1/auth/login" -Method Post -ContentType "application/json" -Body '{"username":"admin1","role":"Admin"}'
$token = $tokenResponse.data

$headers = @{ "Authorization" = "Bearer $token" }

Write-Host "--- Creating Property ---"
$propertyData = @{ title = "Luxury Villa"; price = 1200000; status = "Pending" }
$createResponse = Invoke-RestMethod -Uri "http://localhost:5029/api/v1/admin/property" -Method Post -Headers $headers -ContentType "application/json" -Body ($propertyData | ConvertTo-Json)
Write-Host "Created Property ID: " $createResponse.data.id

Write-Host "--- Archiving Property ---"
Invoke-RestMethod -Uri "http://localhost:5029/api/v1/admin/property/$($createResponse.data.id)" -Method Delete -Headers $headers | Out-Null

Write-Host "--- Fetching Audit Logs ---"
$auditLogs = Invoke-RestMethod -Uri "http://localhost:5029/api/v1/audit" -Method Get -Headers $headers
$auditLogs.data.items | ConvertTo-Json
