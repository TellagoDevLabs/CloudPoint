@echo off

:: BEGIN PROPERTIES
set powershell=C:\WINDOWS\system32\WindowsPowerShell\v1.0\powershell.exe

:: Deploy Settings
set SCRIPTS_FOLDER=.\Scripts
set ASSEMBLY_GAC_FOLDER=..\Lib


:: END PROPERTIES

:: GAC Assemblies
%powershell% -sta %SCRIPTS_FOLDER%\GacAssembly.ps1 -ScriptsFolder %SCRIPTS_FOLDER% -GacFolder %ASSEMBLY_GAC_FOLDER%

IF ERRORLEVEL 1 GOTO ERROR
GOTO END

:ERROR

:END

