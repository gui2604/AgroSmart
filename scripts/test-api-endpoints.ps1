# Smoke / integration tests for AgroSmart API
param([string]$BaseUrl = "http://localhost:8080")

$ErrorActionPreference = "Stop"
$results = [System.Collections.Generic.List[object]]::new()
$failures = [System.Collections.Generic.List[object]]::new()

function Test-Call {
    param(
        [string]$Name,
        [string]$Method = "GET",
        [string]$Path,
        [object]$Body = $null,
        [string]$Token = $null,
        [int[]]$ExpectStatus = @(200),
        [string]$ContentType = "application/json"
    )

    $uri = "$BaseUrl$Path"
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    try {
        $params = @{
            Uri             = $uri
            Method          = $Method
            Headers         = $headers
            UseBasicParsing = $true
        }
        if ($Body -ne $null) {
            $params["Body"] = ($Body | ConvertTo-Json -Depth 10 -Compress)
            $params["ContentType"] = $ContentType
        }

        $response = Invoke-WebRequest @params
        $status = [int]$response.StatusCode
        $ok = $ExpectStatus -contains $status
        $results.Add([pscustomobject]@{ Test = $Name; Method = $Method; Path = $Path; Status = $status; OK = $ok })
        if (-not $ok) {
            $failures.Add([pscustomobject]@{ Test = $Name; Status = $status; Expected = ($ExpectStatus -join ","); Body = $response.Content })
        }
        return $response
    }
    catch {
        $status = 0
        $body = $_.Exception.Message
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode.value__
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $body = $reader.ReadToEnd()
                $reader.Close()
            } catch {}
        }
        $ok = $ExpectStatus -contains $status
        $results.Add([pscustomobject]@{ Test = $Name; Method = $Method; Path = $Path; Status = $status; OK = $ok })
        if (-not $ok) {
            $failures.Add([pscustomobject]@{ Test = $Name; Status = $status; Expected = ($ExpectStatus -join ","); Body = $body })
        }
        if ($status -ge 500) { throw "Server error on $Name : $status - $body" }
        return $null
    }
}

$suffix = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$email = "tester.$suffix@fiap.test"
$password = "TestPass123!"

Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan

# --- Public ---
Test-Call "Root" GET "/"
Test-Call "Health basic" GET "/api/healthcheck"
Test-Call "Health full (no auth)" GET "/api/healthcheck/full" -ExpectStatus @(401)

$r = Test-Call "Register" POST "/api/v1/auth/register" -Body @{
    email = $email; password = $password; role = "Operator"
} -ExpectStatus @(200)
$token = ($r.Content | ConvertFrom-Json).token

Test-Call "Login" POST "/api/v1/auth/login" -Body @{
    email = $email; password = $password
} -ExpectStatus @(200)

Test-Call "Duplicate register" POST "/api/v1/auth/register" -Body @{
    email = $email; password = $password; role = "Operator"
} -ExpectStatus @(400)

Test-Call "Login wrong password" POST "/api/v1/auth/login" -Body @{
    email = $email; password = "wrong-password"
} -ExpectStatus @(400)

Test-Call "Regions without auth" GET "/api/v1/regions" -ExpectStatus @(401)

Test-Call "Health full (auth)" GET "/api/healthcheck/full" -Token $token

# --- Regions ---
$r = Test-Call "List regions" GET "/api/v1/regions" -Token $token
$r = Test-Call "Create region" POST "/api/v1/regions" -Token $token -Body @{
    code = "T$suffix"; name = "Test Bay $suffix"; moduleType = "Hydroponic"; orbitalSegment = "Alpha"
} -ExpectStatus @(201)
$regionId = ($r.Content | ConvertFrom-Json).id

Test-Call "Get region" GET "/api/v1/regions/$regionId" -Token $token
Test-Call "Update region" PUT "/api/v1/regions/$regionId" -Token $token -Body @{
    name = "Test Bay Updated"; moduleType = "Hydroponic"; orbitalSegment = "Alpha"; description = "smoke test"
} -ExpectStatus @(200)

# --- Devices ---
$dev1 = "SENSOR-$suffix-01"
$dev2 = "SENSOR-$suffix-02"

$r = Test-Call "Create device" POST "/api/v1/devices" -Token $token -Body @{
    identifier = $dev1; name = "Sensor A1"; deviceType = "Multi"; firmwareVersion = "1.0"; status = "Active"; regionId = $regionId
} -ExpectStatus @(201)
$deviceId = ($r.Content | ConvertFrom-Json).id

$r = Test-Call "Create device 2" POST "/api/v1/devices" -Token $token -Body @{
    identifier = $dev2; name = "Sensor A2"; deviceType = "Multi"; status = "Active"; regionId = $regionId
} -ExpectStatus @(201)
$deviceId2 = ($r.Content | ConvertFrom-Json).id

Test-Call "List devices" GET "/api/v1/devices" -Token $token
Test-Call "Get device" GET "/api/v1/devices/$deviceId" -Token $token
Test-Call "Devices by region" GET "/api/v1/devices/by-region/$regionId" -Token $token

Test-Call "Duplicate device" POST "/api/v1/devices" -Token $token -Body @{
    identifier = $dev1; name = "Duplicate"; status = "Active"; regionId = $regionId
} -ExpectStatus @(400)

Test-Call "Update device" PUT "/api/v1/devices/$deviceId" -Token $token -Body @{
    name = "Sensor A1 Updated"; deviceType = "Multi"; firmwareVersion = "1.1"; status = "Active"; regionId = $regionId
} -ExpectStatus @(200)

# --- Metric types (seeded) ---
$r = Test-Call "List metric types" GET "/api/v1/metric-types" -Token $token
$metricTypes = $r.Content | ConvertFrom-Json
$co2Id = ($metricTypes | Where-Object { $_.code -eq "CO2" }).id
if (-not $co2Id) { throw "CO2 metric type not found in seed data" }

Test-Call "Get metric type" GET "/api/v1/metric-types/$co2Id" -Token $token

# --- Alert rules ---
$r = Test-Call "Create alert rule" POST "/api/v1/alert-rules" -Token $token -Body @{
    name = "CO2 high $suffix"; description = "test"; metricTypeId = $co2Id; regionId = $regionId
    minThreshold = $null; maxThreshold = 1200; severity = "Warning"; isActive = $true
} -ExpectStatus @(201)
$ruleId = ($r.Content | ConvertFrom-Json).id

Test-Call "List alert rules" GET "/api/v1/alert-rules" -Token $token
Test-Call "Get alert rule" GET "/api/v1/alert-rules/$ruleId" -Token $token
Test-Call "Update alert rule" PUT "/api/v1/alert-rules/$ruleId" -Token $token -Body @{
    name = "CO2 high updated"; description = "test"; metricTypeId = $co2Id; regionId = $regionId
    maxThreshold = 1500; severity = "Critical"; isActive = $true
} -ExpectStatus @(200)

# --- Ingestion ---
$reading = @{
    deviceIdentifier = $dev1
    collectedAt      = "2026-06-04T12:00:00Z"
    measurements     = @(
        @{ metricCode = "TEMPERATURE"; value = 22.4 }
        @{ metricCode = "HUMIDITY"; value = 63.0 }
        @{ metricCode = "CO2"; value = 850 }
        @{ metricCode = "O2"; value = 20.9 }
        @{ metricCode = "LUMINOSITY"; value = 540 }
    )
}
$r = Test-Call "Ingest single" POST "/api/v1/ingestion/readings" -Token $token -Body $reading -ExpectStatus @(200)
$readingId = ($r.Content | ConvertFrom-Json).sensorReadingId

$batch = @(
    $reading,
    @{
        deviceIdentifier = $dev2
        collectedAt      = "2026-06-04T12:05:00Z"
        measurements     = @(
            @{ metricCode = "CO2"; value = 1850 }
            @{ metricCode = "O2"; value = 17.8 }
            @{ metricCode = "TEMPERATURE"; value = 28.6 }
        )
    }
)
Test-Call "Ingest batch" POST "/api/v1/ingestion/readings/batch" -Token $token -Body $batch -ExpectStatus @(200)

$highCo2 = @{
    deviceIdentifier = $dev2
    collectedAt      = "2026-06-04T12:10:00Z"
    measurements     = @(@{ metricCode = "CO2"; value = 2000 })
}
$r = Test-Call "Ingest CO2 alert trigger" POST "/api/v1/ingestion/readings" -Token $token -Body $highCo2 -ExpectStatus @(200)
$triggered = ($r.Content | ConvertFrom-Json).alertsTriggered
if ($triggered -lt 1) {
    $failures.Add([pscustomobject]@{ Test = "CO2 alert trigger"; Status = 200; Expected = "alertsTriggered>=1"; Body = $r.Content })
    $results.Add([pscustomobject]@{ Test = "CO2 alert trigger"; Method = "POST"; Path = "/api/v1/ingestion/readings"; Status = 200; OK = $false })
}

Test-Call "Ingest unknown device" POST "/api/v1/ingestion/readings" -Token $token -Body @{
    deviceIdentifier = "UNKNOWN-$suffix"; measurements = @(@{ metricCode = "CO2"; value = 100 })
} -ExpectStatus @(404)

# Upload JSON file
$sampleFile = Join-Path $PSScriptRoot "..\AgroSmart.Api\SampleData\reading_normal_A1.json"
if (Test-Path $sampleFile) {
    $uploadJson = Get-Content $sampleFile -Raw | ConvertFrom-Json
    $uploadJson.deviceIdentifier = $dev1
    $uploadTemp = [System.IO.Path]::GetTempFileName() + ".json"
    $uploadJson | ConvertTo-Json -Depth 5 | Set-Content $uploadTemp -Encoding UTF8
    $uploadOut = curl.exe -s -w "`n%{http_code}" -X POST "$BaseUrl/api/v1/ingestion/upload" `
        -H "Authorization: Bearer $token" -F "file=@$uploadTemp"
    $uploadLines = $uploadOut -split "`n"
    $uploadStatus = [int]$uploadLines[-1]
    $uploadOk = $uploadStatus -eq 200
    $results.Add([pscustomobject]@{ Test = "Upload JSON file"; Method = "POST"; Path = "/api/v1/ingestion/upload"; Status = $uploadStatus; OK = $uploadOk })
    if (-not $uploadOk) {
        $failures.Add([pscustomobject]@{ Test = "Upload JSON file"; Status = $uploadStatus; Expected = "200"; Body = ($uploadLines[0..($uploadLines.Length-2)] -join "`n") })
        if ($uploadStatus -ge 500) { throw "Server error on Upload JSON file : $uploadStatus" }
    }
    Remove-Item $uploadTemp -Force -ErrorAction SilentlyContinue
}

# --- Readings ---
Test-Call "Get reading" GET "/api/v1/readings/$readingId" -Token $token
Test-Call "Readings by device" GET "/api/v1/readings/by-device/$deviceId" -Token $token

# --- Alerts ---
$r = Test-Call "List alerts" GET "/api/v1/alerts" -Token $token
$alerts = $r.Content | ConvertFrom-Json
$alertId = if ($alerts.Count -gt 0) { $alerts[0].id } else { $null }

if ($alertId) {
    Test-Call "Get alert" GET "/api/v1/alerts/$alertId" -Token $token
    Test-Call "Update alert status" PUT "/api/v1/alerts/$alertId/status" -Token $token -Body @{ status = "Acknowledged" } -ExpectStatus @(200)
}

$r = Test-Call "Create manual alert" POST "/api/v1/alerts" -Token $token -Body @{
    message = "Manual test alert"; deviceId = $deviceId; metricTypeId = $co2Id; triggeredValue = 999; severity = "Info"
} -ExpectStatus @(201)
$manualAlertId = ($r.Content | ConvertFrom-Json).id
Test-Call "Resolve manual alert" PUT "/api/v1/alerts/$manualAlertId/status" -Token $token -Body @{ status = "Resolved" }

Test-Call "List alerts filtered" GET "/api/v1/alerts?regionId=$regionId&status=Open" -Token $token
Test-Call "Delete manual alert" DELETE "/api/v1/alerts/$manualAlertId" -Token $token -ExpectStatus @(204)

# --- Reports ---
Test-Call "Region health report" GET "/api/v1/reports/region-health" -Token $token
Test-Call "Region health by id" GET "/api/v1/reports/region-health/$regionId" -Token $token
Test-Call "Alerts summary" GET "/api/v1/reports/alerts-summary" -Token $token

# --- Negative cases (expect 4xx, not 5xx) ---
Test-Call "Get missing region" GET "/api/v1/regions/999999" -Token $token -ExpectStatus @(404)
Test-Call "Empty batch" POST "/api/v1/ingestion/readings/batch" -Token $token -Body @() -ExpectStatus @(400, 415)
Test-Call "Get missing reading" GET "/api/v1/readings/999999" -Token $token -ExpectStatus @(404)
Test-Call "Get missing alert" GET "/api/v1/alerts/999999" -Token $token -ExpectStatus @(404)

# --- Summary ---
Write-Host "`n=== RESULTS ($($results.Count) calls) ===" -ForegroundColor Cyan
$results | Format-Table -AutoSize

$serverErrors = $results | Where-Object { $_.Status -ge 500 }

if ($serverErrors.Count -gt 0) {
    Write-Host "`nSERVER ERRORS (5xx):" -ForegroundColor Red
    $serverErrors | Format-Table -AutoSize
    exit 1
}

$failed = $results | Where-Object { $_.OK -eq $false }
if ($failed.Count -gt 0) {
    Write-Host "`nFAILED ($($failed.Count)):" -ForegroundColor Yellow
    $failed | Format-Table -AutoSize
    if ($failures.Count -gt 0) { $failures | Format-List }
    exit 1
}

Write-Host "`nAll tests passed (no 5xx, no unexpected status)." -ForegroundColor Green
exit 0
