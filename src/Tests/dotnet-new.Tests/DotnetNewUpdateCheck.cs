﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using FluentAssertions;
using Microsoft.NET.TestFramework.Assertions;
using Microsoft.NET.TestFramework.Commands;
using Xunit.Abstractions;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    public class DotnetNewUpdateCheck : BaseIntegrationTest
    {
        private readonly ITestOutputHelper _log;

        public DotnetNewUpdateCheck(ITestOutputHelper log) : base(log)
        {
            _log = log;
        }

        [Theory]
        [InlineData("--update-check")]
        [InlineData("update --check-only")]
        [InlineData("update --dry-run")]
        public void CanCheckForUpdate(string testCase)
        {
            var home = CreateTemporaryFolder(folderName: "Home");
            new DotnetNewCommand(_log, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0")
                .WithCustomHive(home).WithoutBuiltInTemplates()
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.NotHaveStdOutContaining("Determining projects to restore...")
                .And.HaveStdOutContaining("console")
                .And.HaveStdOutContaining("classlib");

            new DotnetNewCommand(_log, testCase.Split(" "))
                .WithCustomHive(home).WithoutBuiltInTemplates()
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("An update for template packages is available:")
                .And.HaveStdOutMatching("Package\\s+Current\\s+Latest")
                .And.HaveStdOutMatching("Microsoft.DotNet.Common.ProjectTemplates.5.0\\s+5.0.0\\s+([\\d\\.a-z-])+")
                .And.HaveStdOutContaining("To update the package use:")
                .And.HaveStdOutContaining("   dotnet new install <package>::<version>")
                .And.HaveStdOutMatching("   dotnet new install Microsoft\\.DotNet\\.Common\\.ProjectTemplates\\.5\\.0::([\\d\\.a-z-])+");
        }

        [Fact]
        public void ReportsErrorOnUpdateCheckOfLocalPackage()
        {
            string nugetName = "TestNupkgInstallTemplate";
            string nugetVersion = "0.0.1";
            string nugetFullName = $"{nugetName}::{nugetVersion}";
            string nugetFileName = $"{nugetName}.{nugetVersion}.nupkg";
            string workingDirectory = CreateTemporaryFolder();
            var home = CreateTemporaryFolder(folderName: "Home");

            InstallNuGetTemplate(
                Path.Combine(DotnetNewTestPackagesBasePath, nugetFileName),
                _log,
                home,
                workingDirectory);

            new DotnetNewCommand(_log, "--update-check")
                .WithCustomHive(home).WithoutBuiltInTemplates()
                .WithWorkingDirectory(workingDirectory)
                .Execute()
                .Should()
                .Fail()
                .And.HaveStdErr($@"Failed to check update for {nugetFullName}: the package is not available in configured NuGet feeds.


For details on the exit code, refer to https://aka.ms/templating-exit-codes#106");
        }

        [Theory]
        [InlineData("--update-check")]
        [InlineData("update --check-only")]
        [InlineData("update --dry-run")]
        public void DoesNotShowUpdatesWhenAllTemplatesAreUpToDate(string testCase)
        {
            var home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();
            InstallTestTemplate("TemplateResolution/DifferentLanguagesGroup/BasicFSharp", _log, home, workingDirectory);
            new DotnetNewCommand(_log, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0")
                .WithCustomHive(home)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.NotHaveStdOutContaining("Determining projects to restore...")
                .And.HaveStdOutContaining("console")
                .And.HaveStdOutContaining("classlib");

            new DotnetNewCommand(_log, testCase.Split(" "))
                .WithCustomHive(home)
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("All template packages are up-to-date.");
        }

        [Fact]
        public void PrintInfoOnUpdateOnCreation()
        {
            var home = CreateTemporaryFolder(folderName: "Home");
            new DotnetNewCommand(_log, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0")
                .WithCustomHive(home).WithoutBuiltInTemplates()
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Success:")
                .And.HaveStdOutContaining("console")
                .And.HaveStdOutContaining("classlib");

            new DotnetNewCommand(_log, "console")
                  .WithCustomHive(home).WithoutBuiltInTemplates()
                  .WithWorkingDirectory(CreateTemporaryFolder())
                  .Execute()
                  .Should()
                  .ExitWith(0)
                  .And
                  .NotHaveStdErr()
                  .And.HaveStdOutContaining("The template \"Console Application\" was created successfully.")
                  .And.HaveStdOutContaining("An update for template package 'Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0' is available")
                  .And.HaveStdOutContaining("To update the package use:")
                  .And.HaveStdOutMatching("   dotnet new install Microsoft.DotNet.Common.ProjectTemplates.5.0::([\\d\\.a-z-])+");
        }

        [Fact]
        public void DoesNotPrintUpdateInfoOnCreation_WhenNoUpdateCheckOption()
        {
            var home = CreateTemporaryFolder(folderName: "Home");
            new DotnetNewCommand(_log, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0")
                .WithCustomHive(home).WithoutBuiltInTemplates()
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Success:")
                .And.HaveStdOutContaining("console")
                .And.HaveStdOutContaining("classlib");

            new DotnetNewCommand(_log, "console", "--no-update-check", "-o", "no-update-check")
                  .WithCustomHive(home).WithoutBuiltInTemplates()
                  .WithWorkingDirectory(CreateTemporaryFolder())
                  .Execute()
                  .Should()
                  .ExitWith(0)
                  .And
                  .NotHaveStdErr()
                  .And.HaveStdOutContaining("The template \"Console Application\" was created successfully.")
                  .And.NotHaveStdOutContaining("An update for template package 'Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0' is available")
                  .And.NotHaveStdOutContaining("To update the package use:")
                  .And.NotHaveStdOutContaining("   dotnet new --install Microsoft.DotNet.Common.ProjectTemplates.5.0::([\\d\\.a-z-])+");

            new DotnetNewCommand(_log, "console", "-o", "update-check")
                  .WithCustomHive(home).WithoutBuiltInTemplates()
                  .WithWorkingDirectory(CreateTemporaryFolder())
                  .Execute()
                  .Should()
                  .ExitWith(0)
                  .And
                  .NotHaveStdErr()
                  .And.HaveStdOutContaining("The template \"Console Application\" was created successfully.")
                  .And.HaveStdOutContaining("An update for template package 'Microsoft.DotNet.Common.ProjectTemplates.5.0::5.0.0' is available")
                  .And.HaveStdOutContaining("To update the package use:")
                  .And.HaveStdOutMatching("   dotnet new install Microsoft.DotNet.Common.ProjectTemplates.5.0::([\\d\\.a-z-])+");
        }

        [Fact]
        public void DoesNotPrintUpdateInfoOnCreation_WhenLatestVersionIsInstalled()
        {
            var home = CreateTemporaryFolder(folderName: "Home");
            new DotnetNewCommand(_log, "install", "Microsoft.DotNet.Common.ProjectTemplates.5.0")
                .WithCustomHive(home).WithoutBuiltInTemplates()
                .WithWorkingDirectory(CreateTemporaryFolder())
                .Execute()
                .Should()
                .ExitWith(0)
                .And
                .NotHaveStdErr()
                .And.HaveStdOutContaining("Success:")
                .And.HaveStdOutContaining("console")
                .And.HaveStdOutContaining("classlib");

            new DotnetNewCommand(_log, "console")
                  .WithCustomHive(home).WithoutBuiltInTemplates()
                  .WithWorkingDirectory(CreateTemporaryFolder())
                  .Execute()
                  .Should()
                  .ExitWith(0)
                  .And
                  .NotHaveStdErr()
                  .And.HaveStdOutContaining("The template \"Console App\" was created successfully.")
                  .And.NotHaveStdOutMatching("An update for template package 'Microsoft.DotNet.Common.ProjectTemplates.5.0::([\\d\\.a-z-])+' is available")
                  .And.NotHaveStdOutContaining("To update the package use:")
                  .And.NotHaveStdOutMatching("   dotnet new install Microsoft.DotNet.Common.ProjectTemplates.5.0::([\\d\\.a-z-])+");
        }

        [Fact]
        public void CanShowDeprecationMessage_WhenLegacyCommandIsUsed()
        {
            const string deprecationMessage =
@"Warning: use of 'dotnet new --update-check' is deprecated. Use 'dotnet new update --check-only' instead.
For more information, run: 
   dotnet new update -h";

            var home = CreateTemporaryFolder(folderName: "Home");
            var commandResult = new DotnetNewCommand(_log, "--update-check")
                .WithCustomHive(home)
                .Execute();

            commandResult.Should()
                .ExitWith(0)
                .And.NotHaveStdErr();

            Assert.StartsWith(deprecationMessage, commandResult.StdOut);
        }

        [Fact]
        public void DoNotShowDeprecationMessage_WhenNewCommandIsUsed()
        {
            var home = CreateTemporaryFolder(folderName: "Home");
            var commandResult = new DotnetNewCommand(_log, "update", "--check-only")
                .WithCustomHive(home)
                .Execute();

            commandResult.Should()
                .ExitWith(0)
                .And.NotHaveStdErr()
                .And.NotHaveStdOutContaining("Warning")
                .And.NotHaveStdOutContaining("deprecated");
        }
    }
}
