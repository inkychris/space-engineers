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
        public class WalkerSettings
        {
            public WalkerSettings()
            {
                Extension = new PistonSettings();
                Legs = new PistonSettings();
            }

            public PistonSettings Extension;
            public PistonSettings Legs;
        }

        public class Walker
        {
            protected PistonGroup extensionPistons;
            protected PistonGroup frontPistons;
            protected PistonGroup rearPistons;

            protected LandingGearGroup frontGears;
            protected LandingGearGroup rearGears;

            public WalkerSettings Settings;

            protected Hooks ForwardHooks;
            protected Hooks BackwardHooks;

            private Program program;

            public Walker(Program prog, PistonGroup extensionPistons, PistonGroup frontPistons, PistonGroup rearPistons, LandingGearGroup frontGears, LandingGearGroup rearGears)
            {
                program = prog;
                Settings = new WalkerSettings();

                this.extensionPistons = extensionPistons;
                this.frontPistons = frontPistons;
                this.rearPistons = rearPistons;
                this.frontGears = frontGears;
                this.rearGears = rearGears;

                ForwardHooks = new Hooks();
                BackwardHooks = new Hooks();
            }
            
            protected class Hooks
            {
                public Func<bool> Extend = () => true;
                public Func<bool> Extended = () => true;
                public Func<bool> Retract = () => true;
                public Func<bool> Retracted = () => true;
            }

            public bool ExtendAndLockFront()
            {
                frontPistons.Velocity(Settings.Legs.ExtensionVelocity);
                frontPistons.MaxLimit(Settings.Legs.MaxLimit);
                if (frontPistons.Extend())
                {
                    frontGears.Lock();
                    return frontGears.AllLocked();
                }
                return false;
            }

            public bool ExtendAndLockRear()
            {
                rearPistons.Velocity(Settings.Legs.ExtensionVelocity);
                rearPistons.MaxLimit(Settings.Legs.MaxLimit);
                if (rearPistons.Extend())
                {
                    rearGears.Lock();
                    return rearGears.AllLocked();
                }
                return false;
            }

            public bool UnlockAndRetractFront()
            {
                if (rearGears.AllLocked())
                {
                    frontGears.Unlock();
                    program.Echo("Unlock Front");
                }
                frontPistons.Velocity(Settings.Legs.RetractionVelocity);
                frontPistons.MinLimit(Settings.Legs.MinLimit);
                return frontPistons.Retract();
            }

            public bool UnlockAndRetractRear()
            {
                if (frontGears.AllLocked())
                    rearGears.Unlock();
                rearPistons.Velocity(Settings.Legs.RetractionVelocity);
                rearPistons.MinLimit(Settings.Legs.MinLimit);
                return rearPistons.Retract();
            }

            public void Forward()
            {
                program.Echo(System.DateTime.Now.ToString());
                if (extensionPistons.Stopped())
                    extensionPistons.Enable();

                if (extensionPistons.Retracted())
                    if (!ForwardHooks.Retracted())
                        return;

                if (extensionPistons.Extended())
                    if (!ForwardHooks.Extended())
                        return;

                if (extensionPistons.Retracted() || extensionPistons.Extending())
                {
                    program.Echo("Retracted/Extending");
                    if (!ForwardHooks.Extend())
                        return;
                    if (!ExtendAndLockRear())
                    {
                        program.Echo("ExtendAndLockRear");
                        return;
                    }
                    if (!UnlockAndRetractFront())
                    {
                        program.Echo("UnlockAndRetractFront: " + UnlockAndRetractFront());
                        return;
                    }
                    program.Echo("Extend");
                    extensionPistons.MaxLimit(Settings.Extension.MaxLimit);
                    extensionPistons.Velocity(Settings.Extension.ExtensionVelocity);
                    extensionPistons.Extend();
                    return;
                }

                if (extensionPistons.Extended() || extensionPistons.Retracting())
                {
                    program.Echo("Extended/Retracting");
                    if (!ForwardHooks.Retract())
                        return;
                    if (!ExtendAndLockFront())
                    {
                        program.Echo("ExtendAndLockFront");
                        return;
                    }
                    if (!UnlockAndRetractRear())
                    {
                        program.Echo("UnlockAndRetractRear");
                        return;
                    }
                    program.Echo("Retract");
                    extensionPistons.MinLimit(Settings.Extension.MinLimit);
                    extensionPistons.Velocity(Settings.Extension.RetractionVelocity);
                    extensionPistons.Retract();
                    return;
                }
            }

            public void Backward()
            {
                if (extensionPistons.Stopped())
                    extensionPistons.Enable();

                if (extensionPistons.Retracted())
                    if (!BackwardHooks.Retracted())
                        return;

                if (extensionPistons.Extended())
                    if (!BackwardHooks.Extended())
                        return;

                if (extensionPistons.Retracted() || extensionPistons.Extending())
                {
                    if (!BackwardHooks.Extend())
                        return;
                    if (!ExtendAndLockFront())
                        return;
                    if (!UnlockAndRetractRear())
                        return;
                    extensionPistons.Velocity(Settings.Extension.ExtensionVelocity);
                    extensionPistons.Extend();
                    return;
                }

                if (extensionPistons.Extended() || extensionPistons.Retracting())
                {
                    if (!BackwardHooks.Retract())
                        return;
                    if (!ExtendAndLockRear())
                        return;
                    if (!UnlockAndRetractFront())
                        return;
                    extensionPistons.Velocity(Settings.Extension.RetractionVelocity);
                    extensionPistons.Retract();
                    return;
                }
            }
        }
    }
}
