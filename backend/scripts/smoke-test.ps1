<#
.SYNOPSIS
    Sanity-checks all 7 Tic-Tac-Toe API endpoints against a running instance.

.DESCRIPTION
    Start the API first, e.g. from backend/:
        dotnet run --project src/TicTacToe.Api

    Then, in another terminal, run this script:
        powershell -File backend/scripts/smoke-test.ps1
        powershell -File backend/scripts/smoke-test.ps1 -BaseUrl http://localhost:5105

    Compatible with both Windows PowerShell 5.1 and PowerShell 7+.
    Exits non-zero if any check doesn't match the expected status code.
#>
param(
    [string]$BaseUrl = "http://localhost:5105"
)

$ErrorActionPreference = "Stop"
$script:failures = 0

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        $Body = $null
    )

    $requestParams = @{ Method = $Method; Uri = "$BaseUrl$Path"; UseBasicParsing = $true }
    if ($null -ne $Body) {
        $requestParams.Body = ($Body | ConvertTo-Json)
        $requestParams.ContentType = "application/json"
    }

    try {
        $response = Invoke-WebRequest @requestParams
        $parsed = if ($response.Content) { $response.Content | ConvertFrom-Json } else { $null }
        return @{ StatusCode = [int]$response.StatusCode; Body = $parsed }
    } catch {
        $statusCode = [int]$_.Exception.Response.StatusCode
        $parsed = $null
        if ($_.ErrorDetails.Message) {
            try { $parsed = $_.ErrorDetails.Message | ConvertFrom-Json } catch { $parsed = $_.ErrorDetails.Message }
        }
        return @{ StatusCode = $statusCode; Body = $parsed }
    }
}

function Invoke-Check {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [int]$ExpectedStatus,
        $Body = $null
    )

    $result = Invoke-Api -Method $Method -Path $Path -Body $Body

    if ($result.StatusCode -eq $ExpectedStatus) {
        Write-Host "[PASS] $Name -> $($result.StatusCode)" -ForegroundColor Green
    } else {
        Write-Host "[FAIL] $Name -> expected $ExpectedStatus, got $($result.StatusCode)" -ForegroundColor Red
        Write-Host ($result.Body | ConvertTo-Json -Depth 5)
        $script:failures++
    }

    return $result.Body
}

Write-Host "Smoke-testing $BaseUrl" -ForegroundColor Cyan

# 1. POST /api/games (TwoPlayer)
$game = Invoke-Check "Create TwoPlayer game" "POST" "/api/games" 201 @{ mode = "TwoPlayer" }
$gameId = $game.gameId

# 2. GET /api/games/{id}
Invoke-Check "Get game" "GET" "/api/games/$gameId" 200 | Out-Null

# GET unknown id -> 404
Invoke-Check "Get unknown game" "GET" "/api/games/00000000-0000-0000-0000-000000000000" 404 | Out-Null

# 3. POST /api/games/{id}/moves
Invoke-Check "Move X -> cell 0" "POST" "/api/games/$gameId/moves" 200 @{ player = "X"; cellIndex = 0 } | Out-Null
Invoke-Check "Move X again (wrong turn)" "POST" "/api/games/$gameId/moves" 400 @{ player = "X"; cellIndex = 1 } | Out-Null
Invoke-Check "Move O -> cell 0 (occupied)" "POST" "/api/games/$gameId/moves" 400 @{ player = "O"; cellIndex = 0 } | Out-Null
Invoke-Check "Move -> cell 9 (out of range)" "POST" "/api/games/$gameId/moves" 400 @{ player = "O"; cellIndex = 9 } | Out-Null

# 4. POST /api/games/{id}/undo
Invoke-Check "Undo last move" "POST" "/api/games/$gameId/undo" 200 | Out-Null
Invoke-Check "Undo with nothing to undo" "POST" "/api/games/$gameId/undo" 400 | Out-Null

# 5. POST /api/games/{id}/reset
Invoke-Check "Reset game" "POST" "/api/games/$gameId/reset" 200 | Out-Null

# 6. GET /api/scoreboard
Invoke-Check "Get scoreboard" "GET" "/api/scoreboard" 200 | Out-Null

# 7. POST /api/scoreboard/reset
Invoke-Check "Reset scoreboard" "POST" "/api/scoreboard/reset" 200 | Out-Null

# Bonus: VsComputer mode triggers an automatic computer reply move.
$vsComputerGame = Invoke-Check "Create VsComputer game" "POST" "/api/games" 201 @{ mode = "VsComputer" }
$vsGameId = $vsComputerGame.gameId
$afterMove = Invoke-Check "Human move triggers computer reply" "POST" "/api/games/$vsGameId/moves" 200 @{ player = "X"; cellIndex = 4 }
if ($afterMove.moveHistory.Count -eq 2) {
    Write-Host "[PASS] Computer replied automatically (move history has 2 moves)" -ForegroundColor Green
} else {
    Write-Host "[FAIL] Expected 2 moves in history after computer reply, got $($afterMove.moveHistory.Count)" -ForegroundColor Red
    $script:failures++
}

Write-Host ""
if ($script:failures -eq 0) {
    Write-Host "All checks passed." -ForegroundColor Green
    exit 0
} else {
    Write-Host "$($script:failures) check(s) failed." -ForegroundColor Red
    exit 1
}
