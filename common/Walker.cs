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
            public PistonSettings Extension;
            public PistonSettings Legs;
        }

        public class WalkerHooks
        {
            public bool Extend() { return true; }
            public bool Extended() { return true; }
            public bool Retract() { return true; }
            public bool Retracted() { return true; }
        }

        public class Walker
        {
            PistonGroup extensionPistons;
            PistonGroup frontPistons;
            PistonGroup rearPistons;

            LandingGearGroup frontGears;
            LandingGearGroup rearGears;

            public WalkerSettings Settings = new WalkerSettings();
            protected WalkerHooks ForwardHooks = new WalkerHooks();
            protected WalkerHooks BackwardHooks = new WalkerHooks();

            public Walker(PistonGroup extensionPistons, PistonGroup frontPistons, PistonGroup rearPistons, LandingGearGroup frontGears, LandingGearGroup rearGears)
            {
                this.extensionPistons = extensionPistons;
                this.frontPistons = frontPistons;
                this.rearPistons = rearPistons;
                this.frontGears = frontGears;
                this.rearGears = rearGears;
            }

            bool ExtendAndLockFront()
            {
                frontPistons.Velocity(Settings.Legs.ExtensionVelocity);
                frontPistons.MaxLimit(Settings.Legs.MinLimit);
                if (frontPistons.Extend())
                {
                    frontGears.Lock();
                    return frontGears.AllLocked();
                }
                return false;
            }

            bool ExtendAndLockRear()
            {
                rearPistons.Velocity(Settings.Legs.ExtensionVelocity);
                frontPistons.MaxLimit(Settings.Legs.MinLimit);
                if (rearPistons.Extend())
                {
                    rearGears.Lock();
                    return rearGears.AllLocked();
                }
                return false;
            }

            bool UnlockAndRetractFront()
            {
                if (rearGears.AllLocked())
                    frontGears.Unlock();
                frontPistons.Velocity(Settings.Legs.RetractionVelocity);
                frontPistons.MinLimit(Settings.Legs.MaxLimit);
                return frontPistons.Retract();
            }

            bool UnlockAndRetractRear()
            {
                if (frontGears.AllLocked())
                    rearGears.Unlock();
                rearPistons.Velocity(Settings.Legs.RetractionVelocity);
                rearPistons.MinLimit(Settings.Legs.MaxLimit);
                return rearPistons.Retract();
            }

            public void Forward()
            {
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
                    if (!ForwardHooks.Extend())
                        return;
                    if (!ExtendAndLockRear())
                        return;
                    if (!UnlockAndRetractFront())
                        return;
                    extensionPistons.Velocity(Settings.Extension.ExtensionVelocity);
                    extensionPistons.Extend();
                    return;
                }

                if (extensionPistons.Extended() || extensionPistons.Retracting())
                {
                    if (!ForwardHooks.Retract())
                        return;
                    if (!ExtendAndLockFront())
                        return;
                    if (!UnlockAndRetractRear())
                        return;
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
