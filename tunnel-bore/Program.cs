using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
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
        private TunnelBore tunnelBore;
        private ArgParser parser;

        public Program()
        {
            tunnelBore = new TunnelBore(this);
            parser = new ArgParser();

            tunnelBore.Settings.Legs.RetractionVelocity = 0.15f;
            tunnelBore.Settings.Legs.ExtensionVelocity = 0.15f;
            tunnelBore.Settings.Legs.MinLimit = 1.25f;
            tunnelBore.Settings.Legs.MaxLimit = 1.5f;

            tunnelBore.Settings.Extension.ExtensionVelocity = 0.075f;
            tunnelBore.Settings.Extension.ExtensionVelocity = 0.1f;
            tunnelBore.Settings.Extension.MinLimit = 0;
            tunnelBore.Settings.Extension.MaxLimit = 2f;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            string previousInput = "";
            if (argument != "")
            {
                previousInput = parser.Input;
                parser.Input = argument;
            }

            if (parser.Contains("extend"))
            {
                Echo("Extend");
                switch (parser.String("extend"))
                {
                    case "front":
                        Echo("Front");
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        if (!tunnelBore.ExtendAndLockFront())
                            return;
                        Idle();
                        return;
                    case "rear":
                    case "back":
                        Echo("Rear");
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        if (!tunnelBore.ExtendAndLockRear())
                            return;
                        Idle();
                        return;
                    default:
                        Echo("Default");
                        var position = parser.Float("extend");
                        if (position != 0)
                            tunnelBore.GoToRelative(position);
                        else
                            tunnelBore.GoTo(2f);
                        Idle();
                        return;
                }
            }

            if (parser.Contains("retract"))
            {
                Echo("Retract");
                switch (parser.String("retract"))
                {
                    case "front":
                        Echo("Front");
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        if (!tunnelBore.UnlockAndRetractFront())
                            return;
                        Idle();
                        return;
                    case "rear":
                    case "back":
                        Echo("Rear");
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        if (!tunnelBore.UnlockAndRetractRear())
                            return;
                        Idle();
                        return;
                    default:
                        Echo("Default");
                        var position = parser.Float("retract");
                        if (position != 0)
                            tunnelBore.GoToRelative(position);
                        else
                            tunnelBore.GoTo(0);
                        Idle();
                        return;
                }
            }

            if (parser.Contains("enable"))
            {
                switch (parser.String("enable"))
                {
                    case "drill":
                    case "drills":
                        tunnelBore.EnableDrills();
                        if (previousInput.Contains("repeat"))
                            parser.Input = previousInput;
                        else
                            Idle();
                        return;
                    case "welder":
                    case "welders":
                        tunnelBore.EnableWelders();
                        if (previousInput.Contains("repeat"))
                            parser.Input = previousInput;
                        else
                            Idle();
                        return;
                    default:
                        Echo("Unrecognised command!");
                        return;
                }
            }

            if (parser.Contains("disable"))
            {
                switch (parser.String("disable"))
                {
                    case "drill":
                    case "drills":
                        Runtime.UpdateFrequency = UpdateFrequency.Update1;
                        if (!tunnelBore.DisableDrills())
                            return;
                        if (previousInput.Contains("repeat"))
                            parser.Input = previousInput;
                        else
                            Idle();
                        return;
                    case "welder":
                    case "welders":
                        tunnelBore.DisableWelders();
                        if (previousInput.Contains("repeat"))
                            parser.Input = previousInput;
                        else
                            Idle();
                        return;
                    default:
                        Echo("Unrecognised command!");
                        return;
                }
            }

            if (parser.Contains("repeat"))
            {
                switch (parser.String("repeat"))
                {
                    case "forward":
                    case "forwards":
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        tunnelBore.Forward();
                        return;
                    case "back":
                    case "backward":
                    case "backwards":
                        Runtime.UpdateFrequency = UpdateFrequency.Update10;
                        tunnelBore.Backward();
                        return;
                    default:
                        Echo("Unrecognised command!");
                        return;
                }
            }

            if (parser.Contains("drivemode"))
            {
                tunnelBore.DriveMode();
                Idle();
                return;
            }

            if (parser.Contains("walkmode"))
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update10;
                if (!tunnelBore.WalkMode())
                    return;
                Idle();
                return;
            }

            if (parser.Contains("stop|abort"))
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update1;
                if (!tunnelBore.Stop())
                    return;
                Idle();
                return;
            }
        }
        
        public void Idle()
        {
            parser.Input = "idle";
            Runtime.UpdateFrequency = UpdateFrequency.None;
        }
    }
}
