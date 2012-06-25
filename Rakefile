require 'albacore'
require 'version_bumper'


desc "Test"
nunit :test => :build do |nunit|
	nunit.command = "tools/NUnit/nunit-console.exe"
	nunit.assemblies "tests/bin/release/RakeRunner.Tests.dll"
end

desc "Build"
msbuild :build => :assemblyinfo do |msb|
  msb.properties :configuration => :Release
  msb.targets :Clean, :Build
  msb.solution = "RakeRunner.sln"
end


assemblyinfo :assemblyinfo do |asm|
  asm.version = bumper_version.to_s
  asm.file_version = bumper_version.to_s

  asm.company_name = "Self"
  asm.product_name = "Rake Runner"
  asm.copyright = "Shawn Mclean (c) 2012"
  asm.output_file = "AssemblyInfo.cs"
end