@echo off

:: BEGIN PROPERTIES
set powershell=C:\WINDOWS\system32\WindowsPowerShell\v1.0\powershell.exe

:: Deploy Settings
set DATA_FOLDER=.\Data
set SCRIPTS_FOLDER=.\Scripts
set siteUrl=%1
set envcnfg=%SCRIPTS_FOLDER%\Envconfig.ps1

:: END PROPERTIES

:: Deploy wsps packages
%powershell% -sta %SCRIPTS_FOLDER%\PopulateLists.ps1 -SiteUrl %siteUrl% -ScriptsFolder %SCRIPTS_FOLDER% -DataFolderPath %DATA_FOLDER%

IF ERRORLEVEL 1 GOTO ERROR
GOTO END

:ERROR


:END

