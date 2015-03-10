@echo off

rem Now combine apsimsetup.msi and setup.exe into a self extracting installation.
rem This uses IExpress described here:
rem http://www.itscodingtime.com/post/Combine-Setup-MSI-and-EXE-into-a-single-package-with-IExpress.aspx

echo REMEMBER TO RECOMPILE THE .MSI!!!!
IExpress32\iexpress /N Apsoil.sed
pause