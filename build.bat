@echo off

IF NOT EXIST build (
echo Creating build folder
mkdir build
)

cd build
del /Q *.msi
del /Q *.zip

IF NOT EXIST JavLuv (
echo Creating JavLuv folder
mkdir JavLuv
)

cd ../src/

MSBuild JavLuv.sln -t:build -restore -p:RestorePackagesConfig=true /p:Configuration=Release && (
  echo JavLuv Release build succeeded
) || (
  echo Release build failed
  EXIT /B 1
)

cd ../setup/
MSBuild Setup.sln /p:Configuration=Release && (
  echo JavLuv Setup build succeeded
) || (
  echo JavLuv Setup build failed
  EXIT /B 1
)

cd ..

copy /y "setup\bin\Release\Setup-JavLuv.msi" "build\Setup-JavLuv.msi" && (
  echo Copied setup file to build folder
) || (
  echo JavLuv Setup copy failed
  EXIT /B 1
)

copy /y "src\JavLuv\bin\x64\Release\*.dll" "build\JavLuv\"
copy /y "src\JavLuv\bin\x64\Release\*.exe" "build\JavLuv\"
copy /y "src\JavLuv\bin\x64\Release\JavLuv.exe.config" "build\JavLuv\"
copy /y "src\JavLuv\bin\x64\Release\Core14.profile.xml" "build\JavLuv\"
copy /y "src\JavLuv\bin\x64\Release\runtimes\win-x64\native\WebView2Loader.dll" "build\JavLuv\WebView2Loader.dll"

cd build

@for /f "tokens=* usebackq" %%f in (`git tag --sort=committerdate`) do @set "tag=%%f"

set JavLuv=JavLuv-%tag%

rename JavLuv %JavLuv%
rename "Setup-JavLuv.msi" "Setup-%JavLuv%.msi"

tar -a -c -f %JavLuv%.zip %JavLuv%  && (
  echo Copied setup file to build folder
) || (
  echo JavLuv tar archive failed
  EXIT /B 1
)

del /s /q %JavLuv%
rmdir /s /q %JavLuv%

cd ..

EXIT /B 0
