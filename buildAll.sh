

set -e # exit on error

buildTarget='ubuntu.18.04-arm64'
startDir=`pwd`
zip=false;
clean=false;
build=true;
PW=$(cat deploy/password)
# Ex: ./buildAndDeployUpdate.sh -u rock -p 1020 -t ubuntu.18.04-arm64

while getopts "t: zcn" option
do
case "${option}"
in
t) buildTarget="${OPTARG}";;
z) zip=true;;
c) clean=true;;
n) build=false;;
esac
done

USERDIR=$HOME
DEPLOYDIR=$USERDIR/temp
SLOTCONTROLLERDIR=$DEPLOYDIR/bitboss/slotcontroller
SLOTAPIDIR=$DEPLOYDIR/bitboss/slotapi


if [ "$clean" = true ] ; then
	rm -rf ./Code/MainController/obj/
	rm -rf ./Code/MainController/bin/
	rm -rf ./Code/BitBossWebApiController/BitBossWebApiController/obj/
	rm -rf ./Code/BitBossWebApiController/BitBossWebApiController/bin/
	rm -rf ./Code/DLLs/SASConsole/bin/
	rm -rf ./Code/DLLs/SASComms/bin/
	rm -rf ./Code/DLLs/SASComms/obj/
	rm -rf ./Code/PipeTest/obj
	rm -rf ./Code/PipeTest/bin
	rm -rf ./Code/BitBossInterface/BitBossInterface/obj
	rm -rf ./Code/BitBossInterface/BitBossInterface/bin
	rm -rf ./Code/BitBossInterface/SASClientConsole/obj
	rm -rf ./Code/BitBossInterface/SASClientConsole/bin
	rm -rf ./Code/BitbossCardReaderController/obj
	rm -rf ./Code/BitbossCardReaderController/bin
	rm -rf ./Code/DLLs/SASConsole/obj
	# rm ./Code/DLLs/DLLs/SASComms.dll

	# echo quit
	# exit 1
fi

if [ "$build" = true ] ; then
	rm -rf $DEPLOYDIR/bitboss/*
	mkdir -p $SLOTCONTROLLERDIR
	mkdir -p $SLOTAPIDIR
	echo 'Build MainController'
	cd $USERDIR/dev/BITBOSS_SASCONTROLLER/Code/MainController/
	./build.sh $buildTarget
	# echo cp -r bin/Release/net7.0/$buildTarget/* $SLOTCONTROLLERDIR 
	cp -r bin/Release/net7.0/$buildTarget/* $SLOTCONTROLLERDIR 
	rm $USERDIR/dev/BITBOSS_SASCONTROLLER/Code/MainController/*.dll

	echo 'Build SlotApi'
	cd $USERDIR/dev/BITBOSS_SASCONTROLLER/Code/BitBossWebApiController/
	echo `pwd`":./build.sh "$buildTarget $SLOTAPIDIR
	./build.sh $buildTarget
	cp -r BitBossWebApiController/bin/Release/net7.0/$buildTarget/* $SLOTAPIDIR

	rm -rf $SLOTCONTROLLERDIR/publish
	rm -rf $SLOTAPIDIR/publish
	rm -rf $SLOTCONTROLLERDIR/*.pdb
	rm -rf $SLOTAPIDIR/*.pdb
fi

# echo zip $zip
if [ "$zip" = true ] ; then
	cd $DEPLOYDIR/
	# rm -rf $BITBOSSMAINCONTROLLERFOLDER/*.pdb
	# rm -rf $BITBOSSWEBAPIFOLDER/*.pdb
	deployFiles=(
		"bitboss/slotcontroller/appsettings.json"
		"bitboss/slotcontroller/MainController.dll"
		"bitboss/slotapi/appsettings.json"
		"bitboss/slotapi/BitBossWebApiController.dll"
		"bitboss/slotapi/BitBossWebApiController.dll"
		# "bitboss/slotapi/BitBossWebApiController.pdb"
		"bitboss/slotcontroller/BitBossInterface.dll"
		"bitboss/slotcontroller/BitbossCardReaderController.dll"
		"bitboss/slotcontroller/MainController.dll"
		# "bitboss/slotcontroller/MainController.pdb"
		"bitboss/slotcontroller/SASComms.dll"
	)
	echo ${deployFiles[*]}
	version=`cat $USERDIR/dev/BITBOSS_SASCONTROLLER/Code/BitBossWebApiController/version.txt`
	echo Creating bitboss-${version}-${buildTarget}.zip
	mkdir -p $startDir/builds/
	# zip -rq $startDir/builds/bitboss-${version}-${buildTarget}.zip bitboss
	# echo zip -r $startDir/builds/bitboss-${version}-${buildTarget}.zip . -i ${deployFiles[*]}
	ZIPFILE=$startDir/builds/bitboss-${version}-${buildTarget}.zip
	zip -r $ZIPFILE . -i ${deployFiles[*]}
	openssl aes-256-cbc -md sha512 -pbkdf2 -iter 1000000 -in ${ZIPFILE} -out ${ZIPFILE}.enc -pass pass:${PW}
	echo openssl aes-256-cbc -d -md sha512 -pbkdf2 -iter 1000000 -in ${ZIPFILE}.enc -out ${ZIPFILE}.unenc -pass pass:${PW}
fi

cd $startDir

echo -en "\007"
echo build complete