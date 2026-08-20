@echo off
rem Microsoft OpenJDK 17 may require an empty "Packages" directory for Rider plugin instrumentation on Windows.
rem If the build fails with "<JAVA_HOME>\Packages does not exist", create that directory manually.
set "JAVA_HOME=C:\Program Files\Microsoft\jdk-17.0.20.8-hotspot"
set "PATH=%JAVA_HOME%\bin;%PATH%"
set "GRADLE_OPTS=-Dcom.sun.net.ssl.checkRevocation=false -Djavax.net.ssl.trustStoreType=WINDOWS-ROOT"
call gradlew.bat buildPlugin
