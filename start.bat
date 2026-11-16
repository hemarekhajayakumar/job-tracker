@echo off
rem Double-click this file to start Job Tracker.
rem It opens two windows (the API and the website) and then your browser.
rem To stop the app, close those two windows.

cd /d "%~dp0"

where dotnet >nul 2>nul || goto :missing
where npm >nul 2>nul || goto :missing
goto :start

:missing
echo.
echo Windows cannot find .NET or Node.js yet.
echo Restart your computer once, then double-click this file again.
echo.
pause
exit /b 1

:start
echo Starting Job Tracker...

if not exist "frontend\node_modules" (
    echo First run: installing website packages. This takes a minute...
    pushd frontend
    call npm install
    popd
)

start "Job Tracker - API (close to stop)" cmd /k "cd /d "%~dp0backend" && dotnet run"
start "Job Tracker - Website (close to stop)" cmd /k "cd /d "%~dp0frontend" && npm run dev"

echo Waiting for both to start...
timeout /t 12 /nobreak >nul

start "" http://localhost:5173
