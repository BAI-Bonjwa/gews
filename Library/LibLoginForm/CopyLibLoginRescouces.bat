@echo off
REM ���뵱ǰ�������ļ�����Ŀ¼
cd %~dp0
REM ������Դ/�����ļ�
set srcPath=RequiredFiles
xcopy /E /Y %srcPath% %~dp0\bin\Debug\
if errorlevel 0 (echo ������Դ/�����ļ��ɹ�!)^
else (call :PrintError ������Դ/�����ļ�ʧ�ܣ�����!)
call :PrintSeperator
exit

:PrintError
echo %1
pause

:PrintSeperator
echo ======