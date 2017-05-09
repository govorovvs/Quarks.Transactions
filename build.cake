//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Debug");
var nugetSource = Argument("source", "https://nuget.org/api/v2/");
var nugetApiKey =  Argument("apikey", "");

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

string SOLUTION_DIR = Context.Environment.WorkingDirectory.FullPath;
string SOLUTION_PATH = System.IO.Directory.GetFiles(SOLUTION_DIR, "*.sln").Single();
string TOOLS_DIR = SOLUTION_DIR + "/tools";
string PACKAGES_DIR = SOLUTION_DIR + "/packages";
string SOURCE_DIR = SOLUTION_DIR + "/src";
string TESTS_DIR = SOLUTION_DIR + "/tests";
string ARTIFACTS_DIR = SOLUTION_DIR + "/artifacts";

string NUNIT_EXE_PATH = TOOLS_DIR + "/NUnit.ConsoleRunner/tools/nunit3-console.exe";
string NUGET_EXE_PATH = TOOLS_DIR + "/nuget.exe";

//////////////////////////////////////////////////////////////////////
// INITIALIZE
//////////////////////////////////////////////////////////////////////

Task("Initialize")
    .Does(() =>
    {
        CreateDirectory(ARTIFACTS_DIR);
    }
);

//////////////////////////////////////////////////////////////////////
// NUGET
//////////////////////////////////////////////////////////////////////

Task("Restore")
    .Does(() =>
    {
		NuGetRestore(SOLUTION_PATH, 
			new NuGetRestoreSettings 
			{
				ToolPath = NUGET_EXE_PATH
			});
    }
);

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
	{
		MSBuild(SOLUTION_PATH, configurator =>
			configurator.SetConfiguration(configuration)
			.SetVerbosity(Verbosity.Normal)
			.UseToolVersion(MSBuildToolVersion.VS2017)
			.WithTarget("Build"));
	}
);

//////////////////////////////////////////////////////////////////////
// UNIT TESTS
//////////////////////////////////////////////////////////////////////

Task("Tests")
	.IsDependentOn("Initialize")
	.IsDependentOn("Build")
	.Does(() =>
	{	
		string pattern = TESTS_DIR + "/**/bin/" + configuration + "/**/Quarks*.Tests.dll";

		NUnit3Settings settings = new NUnit3Settings 
		{
			ToolPath = NUNIT_EXE_PATH,
			Agents = 1,
			Results = ARTIFACTS_DIR + "/unit-tests.xml"
		};

		NUnit3(pattern, settings);
	}
);

//////////////////////////////////////////////////////////////////////
// PACKAGING
//////////////////////////////////////////////////////////////////////

Task("Pack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		CreateDirectory(PACKAGES_DIR);
		CleanDirectory(PACKAGES_DIR);

		string[] projects = System.IO.Directory.GetFiles(SOURCE_DIR, "*.csproj", SearchOption.AllDirectories);

		foreach(var project in projects)
		{
			MSBuild(project, configurator =>
				configurator.SetConfiguration(configuration)
				.SetVerbosity(Verbosity.Normal)
				.UseToolVersion(MSBuildToolVersion.VS2017)
				.WithTarget("Pack"));
		}

		string[] packages = System.IO.Directory.GetFiles(SOURCE_DIR, "*.nupkg", SearchOption.AllDirectories);
		CopyFiles(packages, PACKAGES_DIR);
	}
);

//////////////////////////////////////////////////////////////////////
// PUBLISHING
//////////////////////////////////////////////////////////////////////

Task("Publish")
	.IsDependentOn("Pack")
	.Does(() =>
	{
		string[] packages = System.IO.Directory.GetFiles(PACKAGES_DIR, "*.nupkg");

		foreach(var package in packages)
		{
			PublishPackage(package);
		}
	}
);

//////////////////////////////////////////////////////////////////////
// HELPER METHODS 
//////////////////////////////////////////////////////////////////////

void PackProject(string projectPath)
{
	var settings = new DotNetCorePackSettings 
	{
		Configuration = configuration,
		Verbose = true,
		OutputDirectory = PACKAGES_DIR
	};

	DotNetCorePack(projectPath, settings);
}

void PublishPackage(string packagePath)
{
	var settings = new NuGetPushSettings 
	{
		Source = nugetSource,
		ApiKey = nugetApiKey,
		ToolPath = NUGET_EXE_PATH
	};

	NuGetPush(packagePath, settings);
}

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);