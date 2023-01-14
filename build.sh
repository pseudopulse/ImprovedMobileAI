rm -rf ImprovedMobileAI/bin/
dotnet restore
dotnet build
rm -rf ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/ImprovedMobileAI/BepInEx/plugins/ImprovedMobileAI
cp -r ImprovedMobileAI/bin/Debug/netstandard2.0  ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/ImprovedMobileAI/BepInEx/plugins/ImprovedMobileAI

rm -rf IMABuild
mkdir IMABuild
cp -r ImprovedMobileAI/bin/Debug/netstandard2.0/* IMABuild
cp manifest.json IMABuild
cp README.md IMABuild
cp icon.png IMABuild
cd IMABuild
rm ../IMA.zip
zip ../IMA.zip *
