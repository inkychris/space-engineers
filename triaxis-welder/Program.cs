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
    partial class Program : MyGridProgram
    {
        private TriaxisPistonGroup pistons;
        private IMyShipWelder welder;

        public Program()
        {
            pistons = LargeTriaxisPistonGroup("X Axis", "Y Axis", "Z Axis");
            welder = GetBlock<IMyShipWelder>("Welder");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var parser = new ArgParser(argument);
            if (parser.Contains("home")) {
                pistons.GoTo(new TriaxisVector(), parser.Float("-v|--velocity", 0.5f));
                return;
            }
            var target = new TriaxisVector(parser.Float("x"), parser.Float("y"), parser.Float("z"));
            pistons.GoTo(target, parser.Float("v", 0.5f));
        }
    }
}
