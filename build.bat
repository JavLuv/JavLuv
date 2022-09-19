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

cd Build

tar -a -c -f JavLuv.zip JavLuv  && (
  echo Copied setup file to build folder
) || (
  echo JavLuv tar archive failed
  EXIT /B 1
)

del /q Javluv
rmdir /q JavLuv

@for /f "tokens=* usebackq" %%f in (`git tag --sort=committerdate`) do @set "tag=%%f"
echo %tag%

rename "JavLuv.zip" "JavLuv-%tag%.zip"
rename "Setup JavLuv.msi" "Setup JavLuv-%tag%.msi"

cd ..

EXIT /B 0
