robocopy %inetroot%\main\bin\Debug %CodeBoxDropShare% *.* /PURGE /IS /IT /V
 
if errorlevel 16  echo  ***FATAL ERROR***  & goto fail
if errorlevel 15  echo FAIL MISM XTRA COPY & goto fail
if errorlevel 14  echo FAIL MISM XTRA      & goto fail
if errorlevel 13  echo FAIL MISM      COPY & goto fail
if errorlevel 12  echo FAIL MISM           & goto fail
if errorlevel 11  echo FAIL      XTRA COPY & goto fail
if errorlevel 10  echo FAIL      XTRA      & goto fail
if errorlevel  9  echo FAIL           COPY & goto fail
if errorlevel  8  echo FAIL                & goto fail
 
if errorlevel  7  echo      MISM XTRA COPY & goto success
if errorlevel  6  echo      MISM XTRA      & goto success
if errorlevel  5  echo      MISM      COPY & goto success
if errorlevel  4  echo      MISM           & goto success
if errorlevel  3  echo           XTRA COPY & goto success
if errorlevel  2  echo           XTRA      & goto success
if errorlevel  1  echo                COPY & goto success
if errorlevel  0  echo    --no change--    & goto success
 
:success
exit /B 0
goto end
 
:fail
exit /B 1
 
:end
