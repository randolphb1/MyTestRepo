@echo off
if "%1"=="" goto OopsOut
shift
"Console File Finder vcpp_cli" %0 %1 %2 %3 %4 %5 %6 %7 %8 %9
goto GetOut
:OopsOut
echo ERROR: Must include at least one text string argument
:GetOut
