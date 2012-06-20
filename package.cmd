%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe Oinq.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

if not exist Download\package\lib\net40 mkdir Download\Package\lib\net40\
if not exist Download\package\lib\net40-client mkdir Download\Package\lib\net40-client\

copy Oinq.Core\bin\Release\Oinq.Core.dll Download\Package\lib\net40\
copy Oinq.Core\bin\Release\Oinq.Core.dll Download\Package\lib\net40-client\

copy Oinq.Core\bin\Release\Oinq.Core.xml Download\Package\lib\net40\
copy Oinq.Core\bin\Release\Oinq.Core.xml Download\Package\lib\net40-client\

.nuget\nuget.exe update -self
.nuget\nuget.exe pack oinq.nuspec -BasePath Download\Package -Output Download