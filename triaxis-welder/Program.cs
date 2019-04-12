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
            var target = new TriaxisVector(
                float.Parse(ParseArg("x", argument)),
                float.Parse(ParseArg("y", argument)),
                float.Parse(ParseArg("z", argument))
            );
            pistons.GoTo(target, 0.5f);
        }
    }
}
