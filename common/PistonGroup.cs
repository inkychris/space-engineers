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
        public class PistonGroup : BlockGroup<IMyPistonBase>
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

            public bool Extend(float velocity = 0.5f)
            {
                Enable();
                bool extended = Extended();
                if (!extended)
                    foreach (IMyPistonBase piston in group)
                    {
                        piston.Velocity = velocity * extensionUnit;
                        piston.Extend();
                    }
                return extended;
            }

            public bool Retract(float velocity = 0.5f)
            {
                Enable();
                bool retracted = Retracted();
                if (!retracted)
                    foreach (IMyPistonBase piston in group)
                    {
                        piston.Velocity = velocity * extensionUnit;
                        piston.Retract();
                    }
                return retracted;
            }

            public void GoTo(float target, float velocity)
            {
                if (target > HighestPosition())
                    target = HighestPosition();
                if (target < LowestPosition())
                    target = LowestPosition();
                if (CurrentPosition() < target)
                {
                    MinLimit(CurrentPosition());
                    MaxLimit(target);
                    Extend(velocity);
                }
                else
                {
                    MinLimit(target);
                    MaxLimit(CurrentPosition());
                    Retract(velocity);
                }
            }

            public void GoToRelative(float offset, float velocity) { GoTo(CurrentPosition() + offset, velocity); }

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
