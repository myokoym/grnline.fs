.\.nuget\NuGet.exe Install Persimmon.Console -Pre -OutputDirectory packages -ExcludeVersion

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe grnline.fs.sln /property:Configuration=Debug /property:VisualStudioVersion=14.0 /target:rebuild

$inputs = @(
  ".\grnline.fs.Persimmon.Tests\bin\Debug\grnline.fs.Persimmon.Tests.dll"
)

.\packages\Persimmon.Console.1.0.1\tools\Persimmon.Console.exe $inputs