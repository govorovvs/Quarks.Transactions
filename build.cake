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

var projects = System.IO.Directory.GetFiles(SOURCE_DIR, "*.csproj", System.IO.SearchOption.AllDirectories);
var testProjects = System.IO.Directory.GetFiles(TESTS_DIR, "*.csproj", System.IO.SearchOption.AllDirectories);

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
		var settings = new DotNetCoreRestoreSettings();

		DotNetCoreRestore(SOLUTION_PATH, settings);
    }
);

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
	{
		foreach(var project in projects)
		{
			BuildProject(project);
		}
	}
);

//////////////////////////////////////////////////////////////////////
// UNIT TESTS
//////////////////////////////////////////////////////////////////////

Task("Test")
	.IsDependentOn("Initialize")
	.IsDependentOn("Build")
	.Does(() =>
	{	
		foreach(var project in testProjects)
		{
			TestProject(project);
		}
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

		foreach (var project in projects)
		{
			PackProject(project);
		}
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

void BuildProject(string projectPath)
{
	var settings = new DotNetCoreBuildSettings
	{
		Configuration = configuration,
		NoIncremental = true
	};

	DotNetCoreBuild(projectPath, settings);
}

void PackProject(string projectPath)
{
	var settings = new DotNetCorePackSettings 
	{
		Configuration = configuration,
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

void TestProject(string projectPath)
{
	var settings = new DotNetCoreTestSettings 
	{
		Configuration = configuration,
		NoBuild = false
	};

	DotNetCoreTest(projectPath, settings);
}

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);