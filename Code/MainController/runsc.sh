pwd
cd ../DLLs/SASComms
dotnet publish -c release 
pwd
cd bin/Release/net7.0
pwd
cp SASComms.dll ../../../../../MainController/
cp SASComms.dll ../../../../../DLLs/DLLs/

cd ../../../../../BitBossInterface/BitBossInterface
dotnet publish -c release 
cd bin/release/net7.0
cp BitBossInterface.dll ../../../../../MainController

cd ../../../../../BitbossCardReaderController
dotnet publish -c release
cd bin/release/net7.0
cp BitbossCardReaderController.dll ../../../../MainController

cd ../../../../MainController
dotnet publish -c release -r $1 --self-contained 

# systemctl stop slotcontroller

cd bin/Release/net7.0/$1
cp -r * $2 

# systemctl start slotcontroller
