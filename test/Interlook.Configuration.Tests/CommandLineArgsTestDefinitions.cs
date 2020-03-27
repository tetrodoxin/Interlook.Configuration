using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Interlook.Configuration.CommandLine;
using Microsoft.Extensions.Configuration;

namespace Interlook.Configuration.Tests
{
    public static class CommandLineArgsTestDefinitions
    {
        public const string ValueTrue = "true";
        public const string ValueYes = "yes";

        public static readonly string Delimiter = ConfigurationPath.KeyDelimiter;
        
        public static readonly string ConfigNameDebugMode = $"debug_mode";
        public static readonly string ConfigNameGeneralQuiet = $"General{Delimiter}quiet";
        public static readonly string ConfigNameGeneralInteractive = $"General{Delimiter}interactive";
        public static readonly string ConfigNameGeneralDoNothing = $"General{Delimiter}donothing";
        public static readonly string ConfigNameSecondVerb = $"Second{Delimiter}verb";
        public static readonly string ConfigNameSecondTarget = $"Second{Delimiter}target";
        public static readonly string ConfigNameSecondMode = $"Second{Delimiter}mode";
        public static readonly string ConfigNameAccountsUser = $"Accounts{Delimiter}user";

        public static readonly List<ArgumentDefinition> Definitions = new List<ArgumentDefinition>
        {
            ArgumentDefinition.CreateSwitchOption('q', "quiet", ConfigNameGeneralQuiet, ValueTrue),
            ArgumentDefinition.CreateSwitchOption('i', ConfigNameGeneralInteractive, ValueTrue),
            ArgumentDefinition.CreateSwitchOption('d', "debug-mode", ConfigNameDebugMode, ValueYes),
            ArgumentDefinition.CreateValueOption('u', "user", ConfigNameAccountsUser, false),
            ArgumentDefinition.CreateSwitchOption('n', "no-user", ConfigNameAccountsUser, ""),
        };

        public static readonly List<ArgumentDefinition> DefinitionsWithValues = new List<ArgumentDefinition>
        {
            ArgumentDefinition.CreateSwitchOption('q', "quiet", ConfigNameGeneralQuiet, ValueTrue),
            ArgumentDefinition.CreateSwitchOption('i', ConfigNameGeneralInteractive, ValueTrue),
            ArgumentDefinition.CreateSwitchOption('d', "debug-mode", ConfigNameDebugMode, ValueYes),
            ArgumentDefinition.CreateValueOption('u', "user", ConfigNameAccountsUser, false),
            ArgumentDefinition.CreateSwitchOption('n', "no-user", ConfigNameAccountsUser, ""),
            ArgumentDefinition.CreateValueOption('m', "mode", ConfigNameSecondMode, false),
            ArgumentDefinition.CreateRequiredValue(ConfigNameSecondVerb),
            ArgumentDefinition.CreateRequiredValue(ConfigNameSecondTarget),
        };


        public static readonly List<ArgumentDefinition> InvalidDefinitions = new List<ArgumentDefinition>
        {
            ArgumentDefinition.CreateSwitchOption('q', "quiet", ConfigNameGeneralQuiet, ValueTrue),
            ArgumentDefinition.CreateSwitchOption('i', "quiet", ConfigNameGeneralInteractive, ValueYes),
        };
    }
}
