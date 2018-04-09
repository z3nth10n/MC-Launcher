@echo off
SonarQube\SonarScanner.MSBuild.exe begin /k:"mc-launcher" /d:sonar.organization="z3nth10n-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="5f905ebe563bcb6ec5f51d1bb7a647abbe26da1e"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MsBuild.exe" "z3nth10n Launcher.sln" /t:Rebuild
SonarQube\SonarScanner.MSBuild.exe end /d:sonar.login="5f905ebe563bcb6ec5f51d1bb7a647abbe26da1e"
pause