@echo off
  setlocal
    set "key=HKLM\SOFTWARE\Microsoft\.NETFramework"
    for /f "tokens=2,*" %%i in (
      '2^>nul reg query %key% /v InstallRoot'
    ) do set "root=%%j"
    <nul set /p "=-- .NET Framework root path . . . "
    if /i "%root%" equ "" goto:err
    if not exist "%root%" goto:err
    echo:found.
    <nul set /p "=-- C# compiler required version . . . "
    for /f "delims=" %%i in (
      'dir /ad/b "%root%" ^| findstr /irc:"v3"'
    ) do (
      if exist "%root%%%i\csc.exe" (
        set "path=%root%%%i"
        set "x=1"
      )
    )
    if not defined x goto:err
    echo:found.
    set "out=%~dp0build"
    set "csc=csc /nologo /t:winexe /out:"%out%\SqliteTester.exe""
    set "csc=%csc% /optimize+ /debug:pdbonly source.cs"
    set "csc=%csc% /r:%~dp0System.Data.SQLite.dll"
    <nul set /p "=-- Building . . . "
    if not exist "%out%" md "%out%"
    2>nul %csc%
    call:getlasterror
    <nul set /p "=-- Copying dependencies . . . "
    pushd "%out%"
      if not exist init.sql copy ..\init.sql >nul
      if not exist *.dll copy ..\*.dll >nul
    popd
    call:getlasterror
  endlocal
exit /b

:err
  echo:not found.
exit /b

:getlasterror
  if %errorlevel% neq 0 (
    echo:failed.
  ) else ( echo:done. )
exit /b
