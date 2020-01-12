#tool nuget:?package=NUnit.ConsoleRunner&version=3.10.0
#tool nuget:?package=GitVersion.CommandLine&version=5.0.0
#addin nuget:?package=Cake.Incubator&version=5.0.1

using System.Xml;
using System.Text.RegularExpressions;
using Cake.Incubator.LoggingExtensions;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

string target = Argument("target", "Default");
string configuration = Argument("configuration", "Release");
string packageVersion = Argument("packageVersion", "1.0.0");

//////////////////////////////////////////////////////////////////////
// SET PACKAGE VERSION
//////////////////////////////////////////////////////////////////////

int dash = packageVersion.IndexOf('-');
string version = dash > 0
    ? packageVersion.Substring(0, dash)
    : packageVersion;

if (configuration == "Debug")
    packageVersion += "-dbg";

//////////////////////////////////////////////////////////////////////
// DETERMINE BUILD ENVIRONMENT
//////////////////////////////////////////////////////////////////////

bool usingXBuild = EnvironmentVariable("USE_XBUILD") != null;

var msBuildSettings = new MSBuildSettings {
    Verbosity = Verbosity.Minimal,
    ToolVersion = MSBuildToolVersion.Default,//The highest available MSBuild tool version//VS2017
    Configuration = configuration,
    PlatformTarget = PlatformTarget.MSIL,
    MSBuildPlatform = MSBuildPlatform.Automatic,
    DetailedSummary = true,
};

var xBuildSettings = new XBuildSettings {
    Verbosity = Verbosity.Minimal,
    ToolVersion = XBuildToolVersion.Default,//The highest available XBuild tool version//NET40
    Configuration = configuration,
};

var nugetRestoreSettings = new NuGetRestoreSettings();
// Older Mono version was not picking up the testcentric source
if (usingXBuild)
    nugetRestoreSettings.Source = new string [] {
        "https://www.myget.org/F/testcentric/api/v2/",
        "https://www.myget.org/F/testcentric/api/v3/index.json",
        "https://www.nuget.org/api/v2/",
        "https://api.nuget.org/v3/index.json",
		"https://www.myget.org/F/nunit/api/v2/",
		"https://www.myget.org/F/nunit/api/v3/index.json"
    };

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

// Directories
string PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
string PACKAGE_DIR = PROJECT_DIR + "package/";
string PACKAGE_TEST_DIR = PACKAGE_DIR + "test/";
string CHOCO_DIR = PROJECT_DIR + "choco/";
string BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";
string BIN_DIR_NET20 = BIN_DIR + "net20/";
string BIN_DIR_NET35 = BIN_DIR + "net35/";

// Packaging
string PACKAGE_NAME = "testcentric-gui";

// Solution
string SOLUTION = "testcentric-gui.sln";

// GUI Testing
string GUI_RUNNER = "testcentric.exe";
string EXPERIMENTAL_RUNNER = "tc-next.exe";
string GUI_TESTS = "TestCentric.Gui.Tests.dll";
string EXPERIMENTAL_TESTS = "Experimental.Gui.Tests.dll";
string ALL_TESTS = "*.Tests.dll";

// Engine Testing
string ENGINE_TESTS = "testcentric.engine.tests";
string[] ENGINE_RUNTIMES = new string[] {"net35", "netcoreapp2.1"};
string ENGINE_CORE_TESTS = "testcentric.engine.core.tests";
string[] ENGINE_CORE_RUNTIMES = IsRunningOnWindows()
	? new string[] {"net35", "netcoreapp2.1", "netcoreapp1.1"}
	: new string[] {"net35", "netcoreapp2.1"};

//////////////////////////////////////////////////////////////////////
// SETUP AND TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
	// TODO: Make GitVersion work on Linux
	if (IsRunningOnWindows())

	{
		var gitVersion = GitVersion();

		Information("GitVersion Properties:");
		Information(gitVersion.Dump());

		string branchName = gitVersion.BranchName;
		// We don't currently use this pattern, but check in case we do later.
		if (branchName.StartsWith ("feature/"))
			branchName = branchName.Substring(8);

		// Default based on GitVersion.yml. This gives us a tag of dev
		// for master, ci for features, pr for pull requests and rc
		// for release branches.
		packageVersion = gitVersion.LegacySemVerPadded;

		// Full release versions and PRs need no further handling
		int dash = packageVersion.IndexOf('-');
		bool isPreRelease = dash > 0;

		string label = gitVersion.PreReleaseLabel;
		bool isPR = label == "pr"; // Set in our GitVersion.yml

		if (isPreRelease && !isPR)
		{
			// This handles non-standard branch names.
			if (label == branchName)
				label = "ci";

			string suffix = "-" + label + gitVersion.CommitsSinceVersionSourcePadded;

			if (label == "ci")
			{
				branchName = Regex.Replace(branchName, "[^0-9A-Za-z-]+", "-");
				suffix += "-" + branchName;
			}

			// Nuget limits "special version part" to 20 chars. Add one for the hyphen.
			if (suffix.Length > 21)
				suffix = suffix.Substring(0, 21);

			packageVersion = gitVersion.MajorMinorPatch + suffix;
		}

		if (BuildSystem.IsRunningOnAppVeyor)
			AppVeyor.UpdateBuildVersion(packageVersion + "-" + AppVeyor.Environment.Build.Number);
	}

    Information("Building {0} version {1} of TestCentric GUI.", configuration, packageVersion);
});

// If we run target Test, we catch errors here in teardown.
// If we run packaging, the CheckTestErrors Task is run instead.
Teardown(context => CheckTestErrors(ref ErrorDetail));

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(BIN_DIR);
});

//////////////////////////////////////////////////////////////////////
// RESTORE NUGET PACKAGES
//////////////////////////////////////////////////////////////////////

Task("RestorePackages")
    .Does(() =>
{
    NuGetRestore(SOLUTION, nugetRestoreSettings);
});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
	.IsDependentOn("Clean")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{
    if(usingXBuild)
    {
        XBuild(SOLUTION, xBuildSettings);
    }
    else
    {
        MSBuild(SOLUTION, msBuildSettings);
    }

    CopyFileToDirectory("LICENSE.txt", BIN_DIR);
    CopyFileToDirectory("NOTICES.txt", BIN_DIR);
    CopyFileToDirectory("CHANGES.txt", BIN_DIR);

	CopyFiles(BIN_DIR_NET20 + "testcentric-agent*", BIN_DIR_NET35);
});

//////////////////////////////////////////////////////////////////////
// TESTS
//////////////////////////////////////////////////////////////////////

var ErrorDetail = new List<string>();

Task("CheckTestErrors")
    .Description("Checks for errors running the test suites")
    .Does(() => CheckTestErrors(ref ErrorDetail));

void CheckTestErrors(ref List<string> errorDetail)
{
    if(errorDetail.Count != 0)
    {
        var copyError = new List<string>();
        copyError = errorDetail.Select(s => s).ToList();
        errorDetail.Clear();
        throw new Exception("One or more unit tests failed, breaking the build.\n"
                              + copyError.Aggregate((x,y) => x + "\n" + y));
    }
}

// Run tests using NUnitLite
private void RunNUnitLite(string testName, string framework)
{
	bool isDotNetCore = framework.StartsWith("netcoreapp");
	string ext = isDotNetCore ? ".dll" : ".exe";
	string testPath = BIN_DIR + framework + "/" + testName + ext;

	int rc = isDotNetCore
		? StartProcess("dotnet", testPath)
		: StartProcess(testPath);

	if (rc > 0)
		ErrorDetail.Add(string.Format($"{testName}: {rc} tests failed running under {framework}"));
	else if (rc < 0)
		ErrorDetail.Add(string.Format($"{testName} returned rc = {rc} running under {framework}"));
}

//////////////////////////////////////////////////////////////////////
// TESTS OF TESTCENTRIC.ENGINE
//////////////////////////////////////////////////////////////////////

var testEngineTask = Task("TestEngine")
	.Description("Tests the TestCentric Engine");

foreach (var runtime in ENGINE_RUNTIMES)
{
	var task = Task("TestEngine_" + runtime)
		.Description("Tests the Engine on " + runtime)
		.IsDependentOn("Build")
		.OnError(exception => { ErrorDetail.Add(exception.Message); })
		.Does(() =>
		{
			RunNUnitLite(ENGINE_TESTS, runtime);
		});

	testEngineTask.IsDependentOn(task);	
}

//////////////////////////////////////////////////////////////////////
// TESTS OF TESTCENTRIC.ENGINE.CORE
//////////////////////////////////////////////////////////////////////

var testEngineCoreTask = Task("TestEngineCore")
	.Description("Tests the TestCentric Engine Core");

foreach (var runtime in ENGINE_CORE_RUNTIMES)
{
	var task = Task("TestEngineCore_" + runtime)
		.Description("Tests the Engine Core on " + runtime)
		.IsDependentOn("Build")
		.OnError(exception => { ErrorDetail.Add(exception.Message); })
		.Does(() =>
		{
			RunNUnitLite(ENGINE_CORE_TESTS, runtime);
		});

	testEngineCoreTask.IsDependentOn(task);	
}

//////////////////////////////////////////////////////////////////////
// TESTS OF THE GUI
//////////////////////////////////////////////////////////////////////

Task("TestGui")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3(BIN_DIR + ALL_TESTS, new NUnit3Settings {
        NoResults = true
        });
});

//////////////////////////////////////////////////////////////////////
// PACKAGING
//////////////////////////////////////////////////////////////////////

var baseFiles = new string[]
{
    BIN_DIR + "LICENSE.txt",
    BIN_DIR + "NOTICES.txt",
    BIN_DIR + "CHANGES.txt",
    BIN_DIR + "testcentric.exe",
    BIN_DIR + "testcentric.exe.config",
    BIN_DIR + "tc-next.exe",
    BIN_DIR + "tc-next.exe.config",
    BIN_DIR + "TestCentric.Common.dll",
    BIN_DIR + "TestCentric.Gui.Components.dll",
    BIN_DIR + "TestCentric.Gui.Runner.dll",
    BIN_DIR + "Experimental.Gui.Runner.dll",
    BIN_DIR + "nunit.uiexception.dll",
    BIN_DIR + "TestCentric.Gui.Model.dll",
    BIN_DIR + "testcentric.engine.api.dll",
    BIN_DIR + "testcentric.engine.metadata.dll",
    BIN_DIR + "testcentric.engine.core.dll",
    BIN_DIR + "testcentric.engine.dll",
    BIN_DIR + "Mono.Cecil.dll",
    BIN_DIR + "testcentric-agent.exe",
    BIN_DIR + "testcentric-agent.exe.config",
    BIN_DIR + "testcentric-agent-x86.exe",
    BIN_DIR + "testcentric-agent-x86.exe.config"
};

var chocoFiles = new string[]
{
    CHOCO_DIR + "VERIFICATION.txt",
    CHOCO_DIR + "testcentric-agent.exe.ignore",
    CHOCO_DIR + "testcentric-agent-x86.exe.ignore",
    CHOCO_DIR + "testcentric.choco.addins"
};

var pdbFiles = new string[]
{
    BIN_DIR + "testcentric.pdb",
    BIN_DIR + "tc-next.pdb",
    BIN_DIR + "TestCentric.Common.pdb",
    BIN_DIR + "TestCentric.Gui.Components.pdb",
    BIN_DIR + "TestCentric.Gui.Runner.pdb",
    BIN_DIR + "Experimental.Gui.Runner.pdb",
    BIN_DIR + "nunit.uiexception.pdb",
    BIN_DIR + "TestCentric.Gui.Model.pdb",
};

//////////////////////////////////////////////////////////////////////
// PACKAGE ZIP
//////////////////////////////////////////////////////////////////////

Task("PackageZip")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CreateDirectory(PACKAGE_DIR);

        var zipFiles = new List<string>(baseFiles);
        if (!usingXBuild)
            zipFiles.AddRange(pdbFiles);

        Zip(BIN_DIR, File(PACKAGE_DIR + PACKAGE_NAME + "-" + packageVersion + ".zip"), zipFiles);
    });

//////////////////////////////////////////////////////////////////////
// PACKAGE CHOCOLATEY
//////////////////////////////////////////////////////////////////////

Task("PackageChocolatey")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CreateDirectory(PACKAGE_DIR);

        var content = new List<ChocolateyNuSpecContent>();
        var sources = usingXBuild
            ? new[] { baseFiles, chocoFiles }
            : new[] { baseFiles, chocoFiles, pdbFiles };

        foreach (var source in sources)
            foreach (string file in source)
                content.Add(new ChocolateyNuSpecContent() { Source = file, Target = "tools" });

        ChocolateyPack(CHOCO_DIR + PACKAGE_NAME + ".nuspec", 
            new ChocolateyPackSettings()
            {
                Version = packageVersion,
                OutputDirectory = PACKAGE_DIR,
                Files = content
            });
    });

Task("CreatePackageTestDirectory")
	.Does(() =>
	{
		CleanDirectory(PACKAGE_TEST_DIR);

		Unzip(File(PACKAGE_DIR + PACKAGE_NAME + "-" + packageVersion + ".zip"), PACKAGE_TEST_DIR);
		CopyTestFiles(BIN_DIR, PACKAGE_TEST_DIR);
	});


//////////////////////////////////////////////////////////////////////
// ZIP PACKAGE TEST
//////////////////////////////////////////////////////////////////////

Task("TestZipPackage")
	.IsDependentOn("PackageZip")
	.IsDependentOn("CreatePackageTestDirectory")
	.Does(() =>
	{
		NUnit3(PACKAGE_TEST_DIR + ALL_TESTS);
	});

//////////////////////////////////////////////////////////////////////
// INTERACTIVE TESTS FOR USE IN DEVELOPMENT
//////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////
// GUI TEST
//////////////////////////////////////////////////////////////////////

Task("GuiTest")
    .IsDependentOn("Build")
    .Does(() =>
{
		StartProcess(BIN_DIR + GUI_RUNNER, BIN_DIR + GUI_TESTS + " --run");
		CheckTestResult("TestResult.xml");
});

//////////////////////////////////////////////////////////////////////
// EXPERIMENTAL GUI TEST
//////////////////////////////////////////////////////////////////////

Task("ExperimentalGuiTest")
    .IsDependentOn("Build")
    .Does(() =>
{
		StartProcess(BIN_DIR + EXPERIMENTAL_RUNNER, BIN_DIR + EXPERIMENTAL_TESTS + " --run");
		CheckTestResult("TestResult.xml");
});

//////////////////////////////////////////////////////////////////////
// ZIP GUI TEST (USES ZIP PACKAGE)
//////////////////////////////////////////////////////////////////////

Task("ZipGuiTest")
	.IsDependentOn("CreatePackageTestDirectory")
	.Does(() =>
	{
		StartProcess(PACKAGE_TEST_DIR + GUI_RUNNER, PACKAGE_TEST_DIR + GUI_TESTS + " --run");
		CheckTestResult("TestResult.xml");
	});

//////////////////////////////////////////////////////////////////////
// ZIP EXPERIMENTAL GUI TEST (USES ZIP PACKAGE)
//////////////////////////////////////////////////////////////////////

Task("ZipExperimentalGuiTest")
	.IsDependentOn("CreatePackageTestDirectory")
	.Does(() =>
	{
		StartProcess(PACKAGE_TEST_DIR + EXPERIMENTAL_RUNNER, PACKAGE_TEST_DIR + EXPERIMENTAL_TESTS + " --run");
		CheckTestResult("TestResult.xml");
	});

//////////////////////////////////////////////////////////////////////
// CHOCOLATEY INSTALL (MUST RUN AS ADMIN)
//////////////////////////////////////////////////////////////////////

Task("ChocolateyInstall")
	.Does(() =>
	{
		if (StartProcess("choco", $"install -f -y -s {PACKAGE_DIR} {PACKAGE_NAME}") != 0)
			throw new Exception("Failed to install package. Must run this command as administrator.");
	});

//////////////////////////////////////////////////////////////////////
// CHOCOLATEY TEST (AFTER INSTALL)
//////////////////////////////////////////////////////////////////////

Task("ChocolateyTest")
	.IsDependentOn("PackageChocolatey")
	.IsDependentOn("CreatePackageTestDirectory")
	.Does(() =>
	{
		// TODO: When starting the commands that chocolatey has shimmed, the StartProcess
		// call returns immediately, so we can't check the test result. For now, we just
		// run the tests and inspect manually but we need to figure out how to wait for
		// the process to complete.
		StartProcess("testcentric", PACKAGE_TEST_DIR + "TestCentric.Gui.Tests.dll --run");
		StartProcess("tc-next", PACKAGE_TEST_DIR + "Experimental.Gui.Tests.dll --run");
		//CheckTestResult("TestResult.xml");
	});

//////////////////////////////////////////////////////////////////////
// HELPER METHODS - TESTS
//////////////////////////////////////////////////////////////////////

// Copy all files needed to run tests from one directory to another
private void CopyTestFiles(string fromDir, string toDir)
{
	CopyFiles(fromDir + "*.Tests.*", toDir);
	CopyFiles(fromDir + "nunit.framework.*", toDir);
	CopyFiles(fromDir + "mock-assembly.*", toDir);
	CopyFiles(fromDir + "test-utilities.*", toDir);
	CopyFiles(fromDir + "System.Threading.Tasks.*", toDir);
	CopyFiles(fromDir + "NSubstitute.*", toDir);
	CopyFiles(fromDir + "Castle.Core.*", toDir);
}

// Examine the result file to make sure a test run passed
private void CheckTestResult(string resultFile)
{
	var doc = new XmlDocument();
	doc.Load(resultFile);

	XmlNode testRun = doc.DocumentElement;
	if (testRun.Name != "test-run")
		throw new Exception("The test-run element was not found.");

	string result = testRun.Attributes["result"]?.Value;
	if (result == null)
		throw new Exception("The test-run element has no result attribute.");

	if (result == "Failed")
	{
		string msg = "The test run failed.";
		string failed = testRun.Attributes["failed"]?.Value;
		if (failed != null)
			msg += $" {int.Parse(failed)} tests failed";
		throw new Exception(msg);
	}
}

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Test")
	.IsDependentOn("TestEngineCore")
	.IsDependentOn("TestEngine")
	.IsDependentOn("TestGui");

Task("Package")
	.IsDependentOn("CheckTestErrors")
    .IsDependentOn("PackageZip")
    .IsDependentOn("PackageChocolatey");

Task("TestPackages")
	.IsDependentOn("TestZipPackage");

Task("Appveyor")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Package")
	.IsDependentOn("TestPackages");

Task("Travis")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("PackageZip")
	.IsDependentOn("TestZipPackage");

Task("All")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Package")
	.IsDependentOn("TestPackages");

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
