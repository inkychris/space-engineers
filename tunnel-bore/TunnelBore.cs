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
        public class TunnelBoreSettings : WalkerSettings
        {
            public float DrillVelocityRPM = 60f;
            public float DrillBreakTorque = 100000f;
            public VRage.MyFixedPoint MinWelderInventoryCount = 50;
            public VRage.MyFixedPoint WelderInventoryTransferCount = 50;
        }

        public class TunnelBore : Walker
        {
            private FunctionalBlockGroup<IMyShipDrill> Drills;
            private IMyMotorStator DrillRotor;

            private FunctionalBlockGroup<IMyShipWelder> Welders;
            private InventoryGroup WelderInventories;

            private IMyShipConnector ConnectorFront;
            private InventoryGroup StorageInventories;

            private IMyCockpit cockpit;

            private MyItemType SteelPlate;

            new public TunnelBoreSettings Settings;

            public TunnelBore(Program parent) : base(parent,
                    extensionPistons: new PistonGroup(parent.GetBlocksFromGroup<IMyPistonBase>("Extension Pistons")),
                    frontPistons: new PistonGroup(parent.GetBlocksFromGroup<IMyPistonBase>("Front Gear Pistons")),
                    rearPistons: new PistonGroup(parent.GetBlocksFromGroup<IMyPistonBase>("Rear Gear Pistons")),
                    frontGears: new LandingGearGroup(parent.GetBlocksFromGroup<IMyLandingGear>("Front Landing Gears")),
                    rearGears: new LandingGearGroup(parent.GetBlocksFromGroup<IMyLandingGear>("Rear Landing Gears"))
                )
            {
                Settings = new TunnelBoreSettings();
                base.Settings = Settings;

                Drills = new FunctionalBlockGroup<IMyShipDrill>(parent.GetBlocksFromGroup<IMyShipDrill>("Drills"));
                DrillRotor = parent.GetBlock<IMyMotorStator>("Drill Rotor");

                Welders = new FunctionalBlockGroup<IMyShipWelder>(parent.GetBlocksFromGroup<IMyShipWelder>("Welders"));
                WelderInventories = new InventoryGroup(parent.GetBlocksFromGroup<IMyEntity>("Welders"));

                ConnectorFront = parent.GetBlock<IMyShipConnector>("Connector Centre Front");
                StorageInventories = new InventoryGroup(parent.GetBlocksFromGroup<IMyEntity>("Storage"));

                cockpit = parent.GetBlock<IMyCockpit>("Cockpit");

                SteelPlate = new MyItemType("MyObjectBuilder_Component", "SteelPlate");
                ForwardHooks.Retracted = Transfer;
                ForwardHooks.Extend = Disconnect;
                BackwardHooks.Extend = Disconnect;
            }

            new protected Hooks ForwardHooks = new Hooks();

            public void EnableDrills()
            {
                DrillRotor.SetValue("LowerLimit", float.MinValue);
                DrillRotor.SetValue("UpperLimit", float.MaxValue);
                Drills.Enable();
                DrillRotor.TargetVelocityRPM = Settings.DrillVelocityRPM;
                DrillRotor.Enabled = true;
                DrillRotor.BrakingTorque = Settings.DrillBreakTorque;
            }

            public bool DisableDrills()
            {
                Drills.Disable();
                DrillRotor.Enabled = false;
                if (Math.Abs(DrillRotor.Angle) < 0.05)
                {
                    DrillRotor.LowerLimitRad = 0f;
                    DrillRotor.UpperLimitRad = 0f;
                    return true;
                }
                return false;
            }

            public void EnableWelders() { Welders.Enable(); }
            public void DisableWelders() { Welders.Disable(); }

            public bool WalkMode()
            {
                frontPistons.MinLimit(Settings.Legs.MaxLimit);
                rearPistons.MinLimit(Settings.Legs.MaxLimit);
                if (!(frontPistons.Extending() && rearPistons.Extending()))
                {
                    frontPistons.Velocity(Settings.Legs.ExtensionVelocity);
                    frontPistons.Extend();
                    rearPistons.Velocity(Settings.Legs.ExtensionVelocity);
                    rearPistons.Extend();
                }
                if (frontPistons.Extended() && rearPistons.Extended())
                {
                    frontGears.Lock();
                    rearGears.Lock();
                    return true;
                }
                return false;
            }

            public bool DriveMode()
            {
                cockpit.HandBrake = true;
                frontPistons.MinLimit(Settings.Legs.MinLimit);
                rearPistons.MinLimit(Settings.Legs.MinLimit);
                if (!(frontPistons.Retracting() && rearPistons.Retracting()))
                {
                    frontPistons.Velocity(0.15f);
                    frontPistons.Retract();
                    rearPistons.Velocity(0.15f);
                    rearPistons.Retract();
                }
                if (!frontGears.AllUnlocked())
                    frontGears.Unlock();
                if (!rearGears.AllUnlocked())
                    rearGears.Unlock();
                return frontPistons.Retracted() && rearPistons.Retracted();
            }

            public bool Connect()
            {
                ConnectorFront.Connect();
                return ConnectorFront.Status == MyShipConnectorStatus.Connected;
            }

            public bool Disconnect()
            {
                ConnectorFront.Disconnect();
                return ConnectorFront.Status != MyShipConnectorStatus.Connected;
            }

            public bool Transfer()
            {
                if (!Connect())
                    return false;
                if (WelderInventories.MinAmountInOne(SteelPlate) < Settings.MinWelderInventoryCount)
                    return false;
                if (!StorageInventories.CanTransferTo(WelderInventories, SteelPlate))
                    return false;
                StorageInventories.TransferTo(WelderInventories, SteelPlate, Settings.WelderInventoryTransferCount);
                WelderInventories.Balance(SteelPlate);
                return Disconnect();
            }

            public void GoTo(float position) { extensionPistons.GoTo(position); }
            public void GoToRelative(float position) { extensionPistons.GoToRelative(position); }

            public bool Stop()
            {
                DisableWelders();
                extensionPistons.Disable();
                if (!DisableDrills())
                    return false;
                return true;
            }
        }
    }
}
