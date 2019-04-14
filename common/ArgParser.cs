using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class ArgParser
        {
            public string Input;
            public System.Text.RegularExpressions.RegexOptions MatchOptions;

            public ArgParser(string input = "")
            {
                Input = input;
                MatchOptions = System.Text.RegularExpressions.RegexOptions.None;
            }

            public bool Contains(string arg)
            {
                string pattern = @"(?:\s|\A)(?:" + arg + @")(?:\s|\z|=)";
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(Input, pattern, MatchOptions);
                return m.Success;
            }

            public string String(string arg, string fallback = "")
            {
                string pattern = @"(?:\s|\A)(?:(?:" + arg + @")[\s=]?)(.+?)(?:\s|\z)";
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(Input, pattern);
                if (m.Success)
                    return m.Groups[1].Value;
                return "";
            }

            public float Float(string arg, float fallback = 0)
            {
                string match = String(arg);
                if (match == "")
                    return fallback;
                float result;
                bool ok = float.TryParse(match, out result);
                if (ok)
                    return result;
                return fallback;
            }
        }
    }
}
