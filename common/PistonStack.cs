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
        public class PistonStack : PistonGroup
        {
            public PistonStack(List<IMyPistonBase> blocklist, float unit = 1) : base(blocklist, unit) { }

            new public void Velocity(float velocity) { base.Velocity(velocity / group.Count); }

            new public bool Extend() { return base.Extend(); }
            new public bool Retract() { return base.Retract(); }

            new public float HighestPosition() { return base.HighestPosition() * group.Count; }
            new public float LowestPosition() { return base.LowestPosition() * group.Count; }
            new public float CurrentPosition() { return base.CurrentPosition() * group.Count; }

            new public void MinLimit(float limit) { base.MinLimit(limit / group.Count); }
            new public void MaxLimit(float limit) { base.MaxLimit(limit / group.Count); }

            new public void GoTo(float target) { base.GoTo(target / group.Count); }
        }
    }
}
