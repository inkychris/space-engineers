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
            public PistonGroup(List<IMyPistonBase> blocklist) : base(blocklist) { }

            private bool Is(PistonStatus status)
            {
                foreach (IMyPistonBase piston in group)
                    if (piston.Status != status)
                        return false;
                return true;

            }

            public bool Stopped() { return Is(PistonStatus.Stopped); }
            public bool Extended() { return Is(PistonStatus.Extended); }
            public bool Extending() { return Is(PistonStatus.Extending); }
            public bool Retracted() { return Is(PistonStatus.Retracted); }
            public bool Retracting() { return Is(PistonStatus.Retracting); }

            public void MinLimit(float limit)
            {
                foreach (IMyPistonBase piston in group)
                    piston.MinLimit = limit;
            }

            public void MaxLimit(float limit)
            {
                foreach (IMyPistonBase piston in group)
                    piston.MaxLimit = limit;
            }

            public float Position() { return group.First().CurrentPosition; }

            public bool Extend(float velocity = 0.5f)
            {
                Enable();
                bool extended = Extended();
                if (!extended)
                    foreach (IMyPistonBase piston in group)
                    {
                        piston.Velocity = velocity;
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
                        piston.Velocity = velocity;
                        piston.Retract();
                    }
                return retracted;
            }
        }
    }
}
