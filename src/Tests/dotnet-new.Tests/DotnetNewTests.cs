﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using FluentAssertions;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.NET.TestFramework.Assertions;
using Microsoft.NET.TestFramework.Commands;
using Xunit.Abstractions;

namespace Microsoft.DotNet.Cli.New.IntegrationTests
{
    [UsesVerify]
    [Collection("Verify Tests")]
    public class DotnetNewTests : BaseIntegrationTest
    {
        private readonly ITestOutputHelper _log;

        public DotnetNewTests(ITestOutputHelper log) : base(log)
        {
            _log = log;
        }

        [Fact]
        public Task CanShowBasicInfo()
        {
            string home = CreateTemporaryFolder(folderName: "Home");
            string workingDirectory = CreateTemporaryFolder();

            CommandResult commandResult = new DotnetNewCommand(_log)
                .WithCustomHive(home)
                .WithWorkingDirectory(workingDirectory)
                .Execute();

            commandResult.Should()
                .ExitWith(0).And.NotHaveStdErr();

            return Verify(commandResult.StdOut).UniqueForOSPlatform();
        }
    }
}
