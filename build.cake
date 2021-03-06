var target = Argument("target", "Default");
var solution = "./Monitor.sln";
var tools = "./tools/Cake/";
var configuration = Argument("configuration", "Debug");

Task("Clean")
    .Does(() =>
    {
        //CleanDirectory(buildDir);
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(solution);
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        if(IsRunningOnWindows())
        {
          // Use MSBuild
          MSBuild(solution, settings =>
            settings.SetConfiguration(configuration));
        }
        else
        {
          // Use XBuild
          XBuild(solution, settings =>
            settings.SetConfiguration(configuration));
        }
    });

Task("Generate-Coverage")
    .IsDependentOn("Build")
    .Does(() =>
    {
        OpenCover(tool => {
          tool.NUnit3("./Monitor.Tests/bin/" + configuration + "/Monitor.Tests.dll", new NUnit3Settings {
                ToolPath = tools + "nunit3-console.exe",
                NoResults = true
              });
          },
          new FilePath("./result.xml"),
          new OpenCoverSettings(){
              ToolPath = tools + "OpenCover.Console.exe",
              ArgumentCustomization = builder => builder.Append("-register:user")
            }
            .WithFilter("+[Monitor.Agent.Console]*")
            .WithFilter("+[Monitor.Dashboard.Nancy]*")
            .WithFilter("+[Monitor.Core]*")
            .WithFilter("+[Monitor.Handlers.Redis]*")
            //Exclude tests project
            .WithFilter("-[Monitor.Tests]*")
        );
    });

Task("Report-Coverage")
    .IsDependentOn("Generate-Coverage")
    .Does(() =>
    {
          ReportGenerator("./result.xml", "./output", new ReportGeneratorSettings() {
              ToolPath = tools + "reportgenerator.exe"
          });
    });


Task("Default")
    .IsDependentOn("Report-Coverage");

RunTarget(target);
