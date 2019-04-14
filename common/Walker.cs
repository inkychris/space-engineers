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
        public class Walker
        {
            public WalkerLegGroup Front;
            public WalkerLegGroup Rear;
            public PistonGroup ExtensionPistons;

            public PistonSettings LegSettings;
            public PistonSettings ForwardSettings;
            public PistonSettings BackwardSettings;

            protected Hooks ForwardHooks;
            protected Hooks BackwardHooks;

            public Walker(Program progam, string BlockGroupPrefix = "")
            {
                Front = new WalkerLegGroup(
                    new PistonGroup(progam.GetBlocksFromGroup<IMyPistonBase>(BlockGroupPrefix + "Front Gear Pistons")),
                    new LandingGearGroup(progam.GetBlocksFromGroup<IMyLandingGear>(BlockGroupPrefix + "Front Landing Gears"))
                );
                Rear = new WalkerLegGroup(
                    new PistonGroup(progam.GetBlocksFromGroup<IMyPistonBase>(BlockGroupPrefix + "Rear Gear Pistons")),
                    new LandingGearGroup(progam.GetBlocksFromGroup<IMyLandingGear>(BlockGroupPrefix + "Rear Landing Gears"))
                );
                ExtensionPistons = new PistonGroup(progam.GetBlocksFromGroup<IMyPistonBase>(BlockGroupPrefix + "Extension Pistons"));

                LegSettings = new PistonSettings();
                Front.Settings = Rear.Settings = LegSettings;

                ForwardSettings = new PistonSettings();
                BackwardSettings = ForwardSettings;

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

            public void Forward() { Walk(Front, Rear, ForwardSettings, ForwardHooks); }
            public void Backward() { Walk(Rear, Front, BackwardSettings, BackwardHooks); }

            private void Walk(WalkerLegGroup front, WalkerLegGroup rear, PistonSettings extSettings, Hooks hooks)
            {
                if (ExtensionPistons.Disabled())
                {
                    if (!front.ExtendAndLock())
                        return;
                    if (!rear.UnlockAndRetract())
                        return;
                    ExtensionPistons.Enable();
                    ExtensionPistons.Retract();
                }

                if (ExtensionPistons.Retracted())
                    if (!hooks.Retracted())
                        return;

                if (ExtensionPistons.Extended())
                    if (!hooks.Extended())
                        return;

                if (ExtensionPistons.Retracted() || ExtensionPistons.Extending())
                {
                    if (!hooks.Extend())
                        return;
                    if (!rear.ExtendAndLock())
                        return;
                    if (!front.UnlockAndRetract())
                        return;
                    ExtensionPistons.MaxLimit(extSettings.MaxLimit);
                    ExtensionPistons.Velocity(extSettings.ExtensionVelocity);
                    ExtensionPistons.Extend();
                    return;
                }

                if (ExtensionPistons.Extended() || ExtensionPistons.Retracting())
                {
                    if (!hooks.Retract())
                        return;
                    if (!front.ExtendAndLock())
                        return;
                    if (!rear.UnlockAndRetract())
                        return;
                    ExtensionPistons.MinLimit(extSettings.MinLimit);
                    ExtensionPistons.Velocity(extSettings.RetractionVelocity);
                    ExtensionPistons.Retract();
                    return;
                }

                if (front.ExtendedAndLocked())
                    ExtensionPistons.Retract();
                if (rear.ExtendedAndLocked())
                    ExtensionPistons.Extend();
            }
        }

        public class WalkerLegGroup
        {
            public PistonGroup Pistons;
            public LandingGearGroup Gears;

            public PistonSettings Settings;

            public WalkerLegGroup(PistonGroup pistons, LandingGearGroup gears)
            {
                Settings = new PistonSettings();
                Pistons = pistons;
                Gears = gears;
            }

            public bool ExtendedAndLocked() { return Pistons.Extended() && Gears.AllLocked(); }

            public bool ExtendAndLock()
            {
                Pistons.Velocity(Settings.ExtensionVelocity);
                Pistons.MaxLimit(Settings.MaxLimit);
                if (Pistons.Extend())
                {
                    Gears.Lock();
                    return Gears.AllLocked();
                }
                return false;
            }

            public bool UnlockedAndRetracted() { return Pistons.Extended() && Gears.AllLocked(); }

            public bool UnlockAndRetract()
            {
                Gears.Unlock();
                Pistons.Velocity(Settings.RetractionVelocity);
                Pistons.MinLimit(Settings.MinLimit);
                return Pistons.Retract();
            }

        }
    }
}
