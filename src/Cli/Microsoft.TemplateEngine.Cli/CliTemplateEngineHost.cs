﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge;

namespace Microsoft.TemplateEngine.Cli
{
    public class CliTemplateEngineHost : DefaultTemplateEngineHost, ICliTemplateEngineHost
    {
        public CliTemplateEngineHost(
            string hostIdentifier,
            string version,
            Dictionary<string, string> preferences,
            IReadOnlyList<(Type InterfaceType, IIdentifiedComponent Instance)> builtIns,
            IReadOnlyList<string>? fallbackHostNames = null,
            string? outputPath = null,
            bool enableVerboseLogging = false)
            : base(
                  hostIdentifier,
                  version,
                  preferences,
                  builtIns,
                  fallbackHostNames,
                  loggerFactory: Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                    builder
                        .SetMinimumLevel(enableVerboseLogging ? LogLevel.Trace : LogLevel.Information)
                        .AddConsole(config => config.FormatterName = nameof(CliConsoleFormatter))
                        .AddConsoleFormatter<CliConsoleFormatter, ConsoleFormatterOptions>(config =>
                        {
                            config.IncludeScopes = true;
                            config.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
                        })))
        {
            string workingPath = FileSystem.GetCurrentDirectory();
            IsCustomOutputPath = outputPath != null;
            OutputPath = outputPath != null ? Path.Combine(workingPath, outputPath) : workingPath;
        }

        public string OutputPath { get; }

        public bool IsCustomOutputPath { get; }

        private bool GlobalJsonFileExistsInPath
        {
            get
            {
                const string fileName = "global.json";
                string? workingPath = OutputPath;
                bool found;
                do
                {
                    string checkPath = Path.Combine(workingPath, fileName);
                    found = FileSystem.FileExists(checkPath);
                    if (!found)
                    {
                        workingPath = Path.GetDirectoryName(workingPath.TrimEnd('/', '\\'));

                        if (string.IsNullOrWhiteSpace(workingPath) || !FileSystem.DirectoryExists(workingPath))
                        {
                            workingPath = null;
                        }
                    }
                }
                while (!found && (workingPath != null));

                return found;
            }
        }

        public override bool TryGetHostParamDefault(string paramName, out string? value)
        {
            switch (paramName)
            {
                case "GlobalJsonExists":
                    value = GlobalJsonFileExistsInPath.ToString();
                    return true;
                default:
                    return base.TryGetHostParamDefault(paramName, out value);
            }
        }

        [Obsolete("Use " + nameof(Logger) + " instead")]
        void ITemplateEngineHost.LogDiagnosticMessage(string message, string category, params string[] details)
        {
            //do nothing if used
            //DefaultTemplateEngineHost may log these messages to Console
        }

        [Obsolete("Use " + nameof(Logger) + " instead")]
        void ITemplateEngineHost.LogTiming(string label, TimeSpan duration, int depth)
        {
            //do nothing if used
            //DefaultTemplateEngineHost may log these messages to Console
        }

        [Obsolete("Use " + nameof(Logger) + " instead")]
        void ITemplateEngineHost.LogMessage(string message)
        {
            //do nothing if used
            //DefaultTemplateEngineHost may log these messages to Console
        }

        [Obsolete("Use CreationStatusResult instead")]
        bool ITemplateEngineHost.OnPotentiallyDestructiveChangesDetected(IReadOnlyList<IFileChange> changes, IReadOnlyList<IFileChange> destructiveChanges)
        {
            //return false to return TemplateCreationResult with CreationResultStatus.DestructiveChangesDetected status.
            return false;
        }
    }
}
