set -e
pwd
Code=`pwd`
buildTarget=$1
BITBOSSWEBAPIFOLDER=$2
DOTNET=net7.0

version=`cat version.txt`
version=$(echo $version | sed 's/[\.]/ /g' )
echo new version $version
read -a myarray <<< $version
((myarray[3]++))
version=$(echo "${myarray[*]}")
echo "version: ${version}"
version=$(echo $version | sed 's/[ ]/\./g' )
echo "version: ${version}"

myversion=$(cat<<EOF
using System.Reflection;

[assembly: AssemblyVersion("${version}")]
EOF
)
echo "${myversion}" > BitBossWebApiController/Version.cs

echo `pwd`":dotnet publish -c release -r "$buildTarget" --self-contained"
dotnet publish -c release -r $buildTarget --self-contained 

if [ -n "${VAR+set}" ]; then
	cd BitBossWebApiController/bin/Release/$DOTNET/$1
	echo cp to $BITBOSSWEBAPIFOLDER
	cp -r * $BITBOSSWEBAPIFOLDER
fi