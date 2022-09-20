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

copy /y "setup\bin\Release\Setup JavLuv.msi" "build\Setup JavLuv.msi" && (
  echo Copied setup file to build folder
) || (
  echo JavLuv Setup copy failed
  EXIT /B 1
)

copy /y "src\JavLuv\bin\Release\*.dll" "build\JavLuv\"
copy /y "src\JavLuv\bin\Release\*.exe" "build\JavLuv\"
copy /y "src\JavLuv\bin\Release\JavLuv.exe.config" "build\JavLuv\"
copy /y "src\JavLuv\bin\Release\Core14.profile.xml" "build\JavLuv\"

cd build

@for /f "tokens=* usebackq" %%f in (`git tag --sort=committerdate`) do @set "tag=%%f"
echo %tag%

set JavLuv=JavLuv-%tag%
echo %JavLuv%

rename JavLuv %JavLuv%
rename "Setup JavLuv.msi" "Setup %JavLuv%.msi"

tar -a -c -f %JavLuv%.zip %JavLuv%  && (
  echo Copied setup file to build folder
) || (
  echo JavLuv tar archive failed
  EXIT /B 1
)

del /q %JavLuv%
rmdir /q %JavLuv%

cd ..

EXIT /B 0
