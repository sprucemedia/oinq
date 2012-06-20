%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe Oinq.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

if not exist Download\package\lib\net4 mkdir Download\Package\lib\net4\
if not exist Download\package\lib\net4-client mkdir Download\Package\lib\net4-client\

copy Oinq.Core\bin\Release\Oinq.Core.dll Download\Package\lib\net4\
copy Oinq.Core\bin\Release\Oinq.Core.dll Download\Package\lib\net4-client\

copy Oinq.Core\bin\Release\Oinq.Core.xml Download\Package\lib\net4\
copy Oinq.Core\bin\Release\Oinq.Core.xml Download\Package\lib\net4-client\

.nuget\nuget.exe update -self
.nuget\nuget.exe pack oinq.nuspec -BasePath Download\Package -Output Download