set -e
pwd
cd ..
Code=`pwd`
buildTarget=$1
BITBOSSMAINCONTROLLERFOLDER=$2
DOTNET=net7.0

cd $Code/DLLs/SASComms
echo `pwd`":dotnet publish -c release"
dotnet publish -c release
cp bin/Release/${DOTNET}/SASComms.dll $Code/MainController/
# cp bin/Release/${DOTNET}/SASComms.dll $Code/DLLs/DLLs/

cd $Code/BitBossInterface/BitBossInterface
echo `pwd`":dotnet publish -c release"
dotnet publish -c release 
cp bin/release/${DOTNET}/BitBossInterface.dll $Code/MainController

cd $Code/BitbossCardReaderController
echo `pwd`":dotnet publish -c release"
dotnet publish -c release
cp bin/release/${DOTNET}/BitbossCardReaderController.dll $Code/MainController

cd $Code/MainController
echo `pwd`":dotnet publish -c release -r "$buildTarget" --self-contained"
dotnet publish -c release -r $buildTarget --self-contained 

# systemctl stop slotcontroller

if test -z "$2" 
then
      echo "\$2 is empty"
else
	cd bin/Release/net7.0/$buildTarget
	cp -r * $BITBOSSMAINCONTROLLERFOLDER 
fi

# systemctl start slotcontroller
