mklink /j Packages ..\WhenTheVersion\Packages
del ..\WhenTheVersion\Packages\Net4X.Serilog.*
rmdir /s /q %userprofile%\.nuget\Packages\Net4X.Serilog
MSBuild.exe Serilog.sln -t:clean
MSBuild.exe Serilog.sln -t:restore -p:RestorePackagesConfig=true
msbuild Serilog.sln -t:restore -p:RestorePackagesConfig=true
MSBuild.exe Serilog.sln -m /property:Configuration=%Configuration% 
git add -A
git commit -a --allow-empty-message -m ''
git push
