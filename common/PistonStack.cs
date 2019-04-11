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

            new public bool Extend(float velocity = 0.5f)
            {
                return base.Extend(velocity / group.Count);
            }

            new public bool Retract(float velocity = 0.5f)
            {
                return base.Retract(velocity / group.Count);
            }

            new public float CurrentPosition()
            {
                float result = 0;
                foreach (IMyPistonBase piston in group)
                {
                    result += piston.CurrentPosition;
                }
                return result;
            }

            new public void MinLimit(float limit)
            {
                base.MinLimit(limit / group.Count);
            }

            new public void MaxLimit(float limit)
            {
                base.MaxLimit(limit / group.Count);
            }

            public void GoTo(float target, float velocity)
            {
                if (target > HighestPosition())
                    target = HighestPosition();
                if (target < LowestPosition())
                    target = LowestPosition();
                MinLimit(target);
                MaxLimit(target);
                if (CurrentPosition() < target)
                    Extend(velocity);
                else
                    Retract(velocity);
            }

            public void GoToRelative(float offset, float velocity)
            {
                GoTo(CurrentPosition() + offset, velocity);
            }
        }
    }
}
