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
        public void Main(string argument, UpdateType updateSource)
        {
            if (argument != "")
                command = argument;

            switch (command)
            {
                case "lock front":
                    FrontGears.Lock();
                    Idle();
                    return;
                case "lock rear":
                    RearGears.Lock();
                    Idle();
                    return;
                case "unlock front":
                    FrontGears.Unlock();
                    Idle();
                    return;
                case "unlock rear":
                    RearGears.Unlock();
                    Idle();
                    return;
                case "extend":
                    ExtensionPistons.Extend();
                    Idle();
                    return;
                case "retract":
                    ExtensionPistons.Retract();
                    Idle();
                    return;
                case "extend front":
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    if (!ExtendAndLockFront())
                        return;
                    Idle();
                    return;
                case "extend rear":
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    if (!ExtendAndLockRear())
                        return;
                    Idle();
                    return;
                case "retract front":
                    UnlockAndRetractFront();
                    Idle();
                    return;
                case "retract rear":
                    UnlockAndRetractRear();
                    Idle();
                    return;
                case "enable drills":
                    StartDrill(60f);
                    Idle();
                    return;
                case "disable drills":
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    if (!StopDrill())
                        return;
                    Idle();
                    return;
                case "enable welders":
                    Welders.Enable();
                    Idle();
                    return;
                case "disable welders":
                    Welders.Disable();
                    Idle();
                    return;
                case "repeat forward":
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    Forward(0.1f);
                    return;
                case "repeat backward":
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    Backward();
                    return;
                case "walkmode":
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    if (!WalkMode())
                        return;
                    FrontGears.Lock();
                    RearGears.Lock();
                    Idle();
                    return;
                case "drivemode":
                    Runtime.UpdateFrequency = UpdateFrequency.Update10;
                    if (!DriveMode())
                        return;
                    Idle();
                    return;
                case "stop":
                    ExtensionPistons.Disable();
                    command = "disable drills";
                    Runtime.UpdateFrequency = UpdateFrequency.Once;
                    return;
                default:
                    Idle();
                    return;
            }
        }

        private PistonGroup ExtensionPistons;
        private PistonGroup FrontGearPistons;
        private PistonGroup RearGearPistons;

        private LandingGearGroup FrontGears;
        private LandingGearGroup RearGears;

        private BlockGroup<IMyShipDrill> Drills;
        private IMyMotorStator DrillRotor;

        private BlockGroup<IMyShipWelder> Welders;

        private IMyCockpit cockpit;

        private IMyShipConnector ConnectorFront;

        private IMyCargoContainer ContainerFront;
        private IMyInventory FrontContainerInventory;
        private IMyCargoContainer ContainerRear;
        private IMyInventory RearContainerInventory;

        private string command = "idle";

        public Program()
        {
            ExtensionPistons = new PistonGroup(GetBlocksFromGroup<IMyPistonBase>("Extension Pistons"));
            FrontGearPistons = new PistonGroup(GetBlocksFromGroup<IMyPistonBase>("Front Gear Pistons"));
            RearGearPistons = new PistonGroup(GetBlocksFromGroup<IMyPistonBase>("Rear Gear Pistons"));

            FrontGears = new LandingGearGroup(GetBlocksFromGroup<IMyLandingGear>("Front Landing Gears"));
            RearGears = new LandingGearGroup(GetBlocksFromGroup<IMyLandingGear>("Rear Landing Gears"));

            Drills = new BlockGroup<IMyShipDrill>(GetBlocksFromGroup<IMyShipDrill>("Drills"));
            DrillRotor = GetBlock<IMyMotorStator>("Drill Rotor");

            Welders = new BlockGroup<IMyShipWelder>(GetBlocksFromGroup<IMyShipWelder>("Welders"));

            cockpit = GetBlock<IMyCockpit>("Cockpit");

            ConnectorFront = GetBlock<IMyShipConnector>("Connector Centre Front");
            ContainerFront = GetBlock<IMyCargoContainer>("Cargo Container Front");
            ContainerRear = GetBlock<IMyCargoContainer>("Cargo Container Rear");
            FrontContainerInventory = ContainerFront.GetInventory();
            RearContainerInventory = ContainerRear.GetInventory();
        }

        public List<T> GetBlocksFromGroup<T>(string groupName) where T : class
        {
            IMyBlockGroup blockGroup = GridTerminalSystem.GetBlockGroupWithName(groupName);
            if (blockGroup == null)
            {
                Echo("Block group '" + groupName + "' not found!");
                return new List<T>();
            }
            List<T> blocks = new List<T>();
            blockGroup.GetBlocksOfType(blocks);
            return blocks;
        }

        public T GetBlock<T>(string name) where T : class
        {
            T block = GridTerminalSystem.GetBlockWithName(name) as T;
            if (block == null)
                Echo("Block '" + name + "' not found!");
            return block;
        }

        public void Idle()
        {
            command = "idle";
            Runtime.UpdateFrequency = UpdateFrequency.None;
        }

        public void Forward(float velocity = 0.1f)
        {
            if (ExtensionPistons.Stopped())
                ExtensionPistons.Enable();

            if (ExtensionPistons.Retracted())
                if (!LockAndTransfer())
                    return;

            Echo("Status: " + ConnectorFront.Status);

            if (ExtensionPistons.Retracted() || ExtensionPistons.Extending())
            {
                if (ConnectorFront.Status == MyShipConnectorStatus.Connected)
                    ConnectorFront.Disconnect();
                if (!ExtendAndLockRear())
                    return;
                if (!UnlockAndRetractFront())
                    return;
                ExtensionPistons.Extend(velocity);
                return;
            }

            if (ExtensionPistons.Extended() || ExtensionPistons.Retracting())
            {
                if (!ExtendAndLockFront())
                    return;
                if (!UnlockAndRetractRear())
                    return;
                ExtensionPistons.Retract();
                return;
            }
        }

        public void Backward()
        {
            if (ExtensionPistons.Stopped())
                ExtensionPistons.Enable();

            if (ExtensionPistons.Retracted() || ExtensionPistons.Extending())
            {
                if (!ExtendAndLockFront())
                    return;
                if (!UnlockAndRetractRear())
                    return;
                ExtensionPistons.Extend();
                return;
            }

            if (ExtensionPistons.Extended() || ExtensionPistons.Retracting())
            {
                if (!ExtendAndLockRear())
                    return;
                if (!UnlockAndRetractFront())
                    return;
                ExtensionPistons.Retract();
                return;
            }
        }

        // Drills
        void StartDrill(float rotorRPM)
        {
            DrillRotor.SetValue("LowerLimit", float.MinValue);
            DrillRotor.SetValue("UpperLimit", float.MaxValue);
            Drills.Enable();
            DrillRotor.TargetVelocityRPM = rotorRPM;
            DrillRotor.Enabled = true;
            DrillRotor.BrakingTorque = 100000f;
        }

        bool StopDrill()
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

        // Whole Landing Gear Legs
        bool ExtendAndLockFront()
        {
            if (FrontGearPistons.Extend())
            {
                FrontGears.Lock();
                return FrontGears.AllLocked();
            }
            return false;
        }

        bool ExtendAndLockRear()
        {
            if (RearGearPistons.Extend())
            {
                RearGears.Lock();
                return RearGears.AllLocked();
            }
            return false;
        }

        bool UnlockAndRetractFront()
        {
            if (RearGears.AllLocked())
                FrontGears.Unlock();
            return FrontGearPistons.Retract();
        }

        bool UnlockAndRetractRear()
        {
            if (FrontGears.AllLocked())
                RearGears.Unlock();
            return RearGearPistons.Retract();
        }

        bool WalkMode()
        {
            if (!(FrontGearPistons.Extending() && RearGearPistons.Extending()))
            {
                FrontGearPistons.Extend(0.1f);
                RearGearPistons.Extend(0.1f);
            }
            if (FrontGearPistons.Extended() && RearGearPistons.Extended())
            {
                FrontGears.Lock();
                RearGears.Lock();
                return true;
            }
            return false;
        }

        bool DriveMode()
        {
            cockpit.HandBrake = true;
            if (!(FrontGearPistons.Retracting() && RearGearPistons.Retracting()))
            {
                FrontGearPistons.Retract(0.2f);
                RearGearPistons.Retract(0.2f);
            }
            if (!FrontGears.AllUnlocked())
                FrontGears.Unlock();
            if (!RearGears.AllUnlocked())
                RearGears.Unlock();
            return FrontGearPistons.Retracted() && RearGearPistons.Retracted();
        }

        // Steel Plate Transfer
        bool LockAndTransfer()
        {
            ConnectorFront.Connect();
            return ConnectorFront.Status == MyShipConnectorStatus.Connected;
        }
    }
}
