@echo off
set "JAVA_HOME=C:\Program Files\Eclipse Adoptium\jdk-17.0.17.10-hotspot"
set "PATH=%JAVA_HOME%\bin;%PATH%"
set "GRADLE_OPTS=-Dcom.sun.net.ssl.checkRevocation=false -Djavax.net.ssl.trustStoreType=WINDOWS-ROOT"
call gradlew.bat buildPlugin
