dotnet publish -c release -r $1 --self-contained 
cd bin/release/net7.0/$1
cp -r * $2 