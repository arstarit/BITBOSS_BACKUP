DIR=`pwd`

if ./build.sh ubuntu.18.04-x64 ; then
	echo "Build succeeded"
else
	echo "Build failed"
	exit
fi

cd $DIR

./BitBossWebApiController/bin/Release/net7.0/ubuntu.18.04-x64/BitBossWebApiController
#--urls=http://0.0.0.0:5002

