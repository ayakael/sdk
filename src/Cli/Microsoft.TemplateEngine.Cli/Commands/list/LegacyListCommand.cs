﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.TemplateEngine.Abstractions;

namespace Microsoft.TemplateEngine.Cli.Commands
{
    internal class LegacyListCommand : BaseListCommand
    {
        public LegacyListCommand(
            NewCommand parentCommand,
            Func<ParseResult, ITemplateEngineHost> hostBuilder)
            : base(parentCommand, hostBuilder, "--list")
        {
            this.IsHidden = true;
            this.AddAlias("-l");
            AddValidator(ValidateParentCommandArguments);

            parentCommand.AddNoLegacyUsageValidators(this, except: Filters.Values.Concat(new Symbol[] { ColumnsAllOption, ColumnsOption, NewCommand.ShortNameArgument }).ToArray());
        }

        public override Option<bool> ColumnsAllOption => ParentCommand.ColumnsAllOption;

        public override Option<string[]> ColumnsOption => ParentCommand.ColumnsOption;

        protected override Option GetFilterOption(FilterOptionDefinition def)
        {
            return ParentCommand.LegacyFilters[def];
        }

        protected override Task<NewCommandStatus> ExecuteAsync(ListCommandArgs args, IEngineEnvironmentSettings environmentSettings, InvocationContext context)
        {
            PrintDeprecationMessage<LegacyListCommand, ListCommand>(args.ParseResult);
            return base.ExecuteAsync(args, environmentSettings, context);
        }

        private void ValidateParentCommandArguments(CommandResult commandResult)
        {
            var nameArgumentResult = commandResult.Children.FirstOrDefault(symbol => symbol.Symbol == this.NameArgument);
            if (nameArgumentResult == null)
            {
                return;
            }
            ParentCommand.ValidateShortNameArgumentIsNotUsed(commandResult);
        }
    }
}
