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
        public struct TriaxisCoord
        {
            public float x, y, z;

            public static TriaxisCoord operator +(TriaxisCoord first, TriaxisCoord second)
            {
                return new TriaxisCoord
                {
                    x = first.x + second.x,
                    y = first.y + second.y,
                    z = first.z + second.z
                };
            }
        }

        public class TriaxisPistonGroup
        {
            private PistonStack xAxis;
            private PistonStack yAxis;
            private PistonStack zAxis;
            private float velocity;

            public TriaxisPistonGroup(PistonStack x, PistonStack y, PistonStack z)
            {
                xAxis = x;
                yAxis = y;
                zAxis = z;
                velocity = 1f;
            }

            public TriaxisCoord CurrentPosition()
            {
                return new TriaxisCoord {
                    x = xAxis.CurrentPosition(),
                    y = yAxis.CurrentPosition(),
                    z = zAxis.CurrentPosition(),
                };
            }

            public void GoTo(TriaxisCoord target)
            {
                xAxis.GoTo(target.x, velocity);
                yAxis.GoTo(target.y, velocity);
                zAxis.GoTo(target.z, velocity);
            }

            public void GoToRelative(TriaxisCoord offset)
            {
                TriaxisCoord target = CurrentPosition() + offset;
                xAxis.GoTo(target.x, velocity);
                yAxis.GoTo(target.y, velocity);
                zAxis.GoTo(target.z, velocity);
            }
        }

        public TriaxisPistonGroup LargeTriaxisPistonGroup(string xAxisGroup, string yAxisGroup, string zAxisGroup)
        {
            float unit = 2.5f;
            return new TriaxisPistonGroup(
                new PistonStack(GetBlocksFromGroup<IMyPistonBase>(xAxisGroup), unit),
                new PistonStack(GetBlocksFromGroup<IMyPistonBase>(yAxisGroup), unit),
                new PistonStack(GetBlocksFromGroup<IMyPistonBase>(zAxisGroup), unit)
            );
        }
    }
}
