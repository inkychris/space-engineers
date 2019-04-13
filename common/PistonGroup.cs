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
    partial class Program
    {
        public class PistonSettings
        {
            public float ExtensionVelocity = 0.5f;
            public float RetractionVelocity = 0.5f;
            public float MinLimit = 0;
            public float MaxLimit = 1;
        }

        public class PistonGroup : FunctionalBlockGroup<IMyPistonBase>
        {
            private float extensionUnit;

            public PistonGroup(List<IMyPistonBase> blocklist, float unit = 1) : base(blocklist) { extensionUnit = unit; }

            public bool Stopped() { return Is(PistonStatus.Stopped); }
            public bool Extended() { return Is(PistonStatus.Extended); }
            public bool Extending() { return Is(PistonStatus.Extending); }
            public bool Retracted() { return Is(PistonStatus.Retracted); }
            public bool Retracting() { return Is(PistonStatus.Retracting); }

            public float HighestPosition() { return group.First().HighestPosition / extensionUnit; }
            public float LowestPosition() { return group.First().LowestPosition / extensionUnit; }
            public float CurrentPosition() { return group.First().CurrentPosition / extensionUnit; }

            public void MinLimit(float limit)
            {
                foreach (IMyPistonBase piston in group)
                    piston.MinLimit = limit * extensionUnit;
            }

            public void MaxLimit(float limit)
            {
                foreach (IMyPistonBase piston in group)
                    piston.MaxLimit = limit * extensionUnit;
            }

            public void Velocity(float velocity)
            {
                foreach (IMyPistonBase piston in group)
                    piston.Velocity = velocity * extensionUnit;
            }

            public bool Extend()
            {
                Enable();
                bool extended = Extended();
                if (!extended)
                    foreach (IMyPistonBase piston in group)
                        piston.Extend();
                return extended;
            }

            public bool Retract()
            {
                Enable();
                bool retracted = Retracted();
                if (!retracted)
                    foreach (IMyPistonBase piston in group)
                        piston.Retract();
                return retracted;
            }

            public void GoTo(float target)
            {
                if (target > HighestPosition())
                    target = HighestPosition();
                if (target < LowestPosition())
                    target = LowestPosition();
                if (CurrentPosition() < target)
                {
                    MinLimit(CurrentPosition());
                    MaxLimit(target);
                    Extend();
                }
                else
                {
                    MinLimit(target);
                    MaxLimit(CurrentPosition());
                    Retract();
                }
            }

            public void GoToRelative(float offset) { GoTo(CurrentPosition() + offset); }

            private bool Is(PistonStatus status)
            {
                foreach (IMyPistonBase piston in group)
                    if (piston.Status != status)
                        return false;
                return true;
            }
        }
    }
}
