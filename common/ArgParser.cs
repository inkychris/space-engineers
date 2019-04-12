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
            private string arguments;
            public ArgParser(string arguments) { this.arguments = arguments; }

            public bool Contains(string arg)
            {
                string pattern = @"(?:\s|\A)(?:" + arg + @")(?:\s|\z|=)";
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(arguments, pattern);
                return m.Success;
            }

            public string String(string arg, string fallback = "")
            {
                string pattern = @"(?:\s|\A)(?:(?:" + arg + @")[\s=]?)(.+?)(?:\s|\z)";
                System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(arguments, pattern);
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
