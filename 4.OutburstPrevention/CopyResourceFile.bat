@echo off
REM ���뵱ǰ�������ļ�����Ŀ¼
cd %~dp0
REM �����ϼ�Ŀ¼
cd ..
REM ������Դ/�����ļ�
set srcPath=4.RC
xcopy /E /Y ResourceFiles\%srcPath% %~dp0
xcopy /E /Y ResourceFiles\%srcPath% %~dp0\bin\Debug\
xcopy /E /Y ResourceFiles\%srcPath% %~dp0\bin\Release\
if errorlevel 0 (echo ������Դ/�����ļ��ɹ�!)^
else (call :PrintError ������Դ/�����ļ�ʧ�ܣ�����!)
call :PrintSeperator

REM ���Ƹ���ϵͳ����Ҫ���ļ�
xcopy /E /Y ResourceFiles\AllRequired %~dp0
xcopy /E /Y ResourceFiles\AllRequired %~dp0\bin\Debug\
xcopy /E /Y ResourceFiles\AllRequired %~dp0\bin\Release\

if errorlevel 0 (echo ����ͨ����Դ/�����ļ��ɹ�!)^
else (call :PrintError ����ͨ����Դ/�����ļ�ʧ�ܣ�����!)
call :PrintSeperator

exit

:PrintError
echo %1
pause

:PrintSeperator
echo ======
pause