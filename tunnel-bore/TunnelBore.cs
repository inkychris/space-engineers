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
        public class TunnelBoreSettings
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

            public TunnelBoreSettings Settings;

            private Program program;

            public TunnelBore(Program program) : base(program)
            {
                this.program = program;
                Settings = new TunnelBoreSettings();

                Drills = new FunctionalBlockGroup<IMyShipDrill>(program.GetBlocksFromGroup<IMyShipDrill>("Drills"));
                DrillRotor = program.GetBlock<IMyMotorStator>("Drill Rotor");

                Welders = new FunctionalBlockGroup<IMyShipWelder>(program.GetBlocksFromGroup<IMyShipWelder>("Welders"));
                WelderInventories = new InventoryGroup(program.GetBlocksFromGroup<IMyEntity>("Welders"));

                ConnectorFront = program.GetBlock<IMyShipConnector>("Connector Centre Front");
                StorageInventories = new InventoryGroup(program.GetBlocksFromGroup<IMyEntity>("Storage"));

                cockpit = program.GetBlock<IMyCockpit>("Cockpit");

                SteelPlate = new MyItemType("MyObjectBuilder_Component", "SteelPlate");

                ForwardHooks.Retracted = Transfer;
                ForwardHooks.Extend = () => { WelderInventories.Balance(SteelPlate); return Disconnect(); };
                BackwardHooks.Extend = Disconnect;
            }

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
                Front.Pistons.MinLimit(LegSettings.MinLimit);
                Rear.Pistons.MinLimit(LegSettings.MaxLimit);
                if (!(Front.Pistons.Extending() && Rear.Pistons.Extending()))
                {
                    Front.Pistons.Velocity(LegSettings.ExtensionVelocity);
                    Front.Pistons.Extend();
                    Rear.Pistons.Velocity(LegSettings.ExtensionVelocity);
                    Rear.Pistons.Extend();
                }
                if (Front.Pistons.Extended() && Rear.Pistons.Extended())
                {
                    Front.Gears.Lock();
                    Rear.Gears.Lock();
                    return true;
                }
                return false;
            }

            public bool DriveMode()
            {
                cockpit.HandBrake = true;
                Front.Pistons.MinLimit(0);
                Rear.Pistons.MinLimit(0);
                if (!(Front.Pistons.Retracting() && Rear.Pistons.Retracting()))
                {
                    Front.Pistons.Velocity(LegSettings.RetractionVelocity);
                    Front.Pistons.Retract();
                    Rear.Pistons.Velocity(LegSettings.RetractionVelocity);
                    Rear.Pistons.Retract();
                }
                Front.Gears.Unlock();
                Rear.Gears.Unlock();
                return Front.Pistons.Retracted() && Rear.Pistons.Retracted();
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
                Connect();
                if (WelderInventories.ItemAmount(SteelPlate) >= Welders.Count() * Settings.MinWelderInventoryCount)
                    return Disconnect();
                if (!StorageInventories.CanTransferTo(WelderInventories, SteelPlate))
                    return false;
                StorageInventories.TransferTo(WelderInventories, SteelPlate, Welders.Count() * Settings.WelderInventoryTransferCount);
                WelderInventories.Balance(SteelPlate);
                if (WelderInventories.MinAmountInOne(SteelPlate) < Settings.MinWelderInventoryCount)
                {
                    program.Echo("Not enough steel plate in welder(s)!");
                    return false;
                }
                return Disconnect();
            }

            public void GoTo(float position) { ExtensionPistons.GoTo(position); }
            public void GoToRelative(float position) { ExtensionPistons.GoToRelative(position); }

            public bool Stop()
            {
                DisableWelders();
                ExtensionPistons.Disable();
                if (!DisableDrills())
                    return false;
                return true;
            }
        }
    }
}
