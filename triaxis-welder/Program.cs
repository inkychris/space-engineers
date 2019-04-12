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
        private IMyProgrammableBlock self;
        private TriaxisPistonGroup pistons;
        private IMyShipWelder welder;
        private ArgParser parser;
        private string[] commands;
        private int commandIndex;

        public Program()
        {
            self = GetBlock<IMyProgrammableBlock>("Triaxis Welder Controller");
            pistons = LargeTriaxisPistonGroup("X Axis", "Y Axis", "Z Axis");
            welder = GetBlock<IMyShipWelder>("Welder");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument != "")
            {
                commands = new string[] { argument };
                commandIndex = 0;
                parser = new ArgParser(commands[commandIndex]);
            }

            if (parser.Contains("home"))
            {
                pistons.GoTo(new TriaxisVector(), parser.Float("-v|--velocity", 0.5f));
                return;
            }

            if (parser.Contains("custom"))
            {
                commands = self.CustomData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                commandIndex = 0;
                parser = new ArgParser(commands[commandIndex]);
            }

            if (commandIndex < commands.Length)
            {
                Echo("Running command" + commandIndex);
                Runtime.UpdateFrequency = UpdateFrequency.Update10;
                if (GoTo())
                {
                    commandIndex++;
                    if (commandIndex == commands.Length)
                        return;
                    parser = new ArgParser(commands[commandIndex]);
                    Echo("Incrementing cmd index");
                    return;
                }
            }
            else
            {
                Runtime.UpdateFrequency = UpdateFrequency.None;
                Echo("Done!");
            }
        }

        public bool Home()
        {
            return pistons.Stopped();
        }

        public bool GoTo()
        {
            var target = new TriaxisVector(parser.Float("x"), parser.Float("y"), parser.Float("z"));
            pistons.GoTo(target, parser.Float("-v|--velocity", 0.5f));
            return pistons.CurrentPosition().Equals(target);
        }
    }
}
