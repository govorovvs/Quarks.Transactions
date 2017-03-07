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
string TOOLS_DIR = SOLUTION_DIR + "/tools";
string PACKAGES_DIR = SOLUTION_DIR + "/packages";
string SOURCE_DIR = SOLUTION_DIR + "/src";
string TESTS_DIR = SOLUTION_DIR + "/tests";
string ARTIFACTS_DIR = SOLUTION_DIR + "/artifacts";

string NUGET_EXE_PATH = TOOLS_DIR + "/nuget.exe";

string[] projects = System.IO.Directory.GetDirectories(SOURCE_DIR);
string[] testProjects = System.IO.Directory.GetDirectories(TESTS_DIR);

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
        DotNetCoreRestore();
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

Task("Tests")
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
	.Does(() =>
	{
		CleanDirectories(PACKAGES_DIR);

		foreach(var project in projects)
		{
			PackProject(project);
		}

		DeleteFiles(PACKAGES_DIR + "/*.symbols.nupkg");
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

void TestProject(string projectPath)
{
	var settings = new DotNetCoreTestSettings 
	{
		Configuration = configuration,
		NoBuild = false
	};

	DotNetCoreTest(projectPath, settings);
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

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);