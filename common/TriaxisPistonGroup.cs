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
        public class TriaxisPistonGroup
        {
            private PistonStack xAxis;
            private PistonStack yAxis;
            private PistonStack zAxis;

            public TriaxisPistonGroup(PistonStack x, PistonStack y, PistonStack z)
            {
                xAxis = x;
                yAxis = y;
                zAxis = z;
            }

            public bool Stopped() { return xAxis.Stopped() && yAxis.Stopped() && zAxis.Stopped(); }

            public TriaxisVector CurrentPosition()
            {
                return new TriaxisVector(
                    xAxis.CurrentPosition(),
                    yAxis.CurrentPosition(),
                    zAxis.CurrentPosition()
                );
            }

            public TriaxisVector TranslationVector(TriaxisVector target)
            {
                return target - CurrentPosition();
            }

            public TriaxisVector VelocityVector(TriaxisVector target, float velocity)
            {
                var translationVector = TranslationVector(target);
                float directDistance = (float) Math.Sqrt(Math.Pow(translationVector.x, 2) + Math.Pow(translationVector.y, 2) + Math.Pow(translationVector.z, 2));
                if (directDistance == 0)
                    return new TriaxisVector();
                return translationVector * (velocity / directDistance);
            }

            public void GoTo(TriaxisVector target, float velocity)
            {
                var velocityVector = VelocityVector(target, velocity);
                xAxis.GoTo(target.x, velocityVector.x);
                yAxis.GoTo(target.y, velocityVector.y);
                zAxis.GoTo(target.z, velocityVector.z);
            }

            public void GoToRelative(TriaxisVector offset, float velocity)
            {
                TriaxisVector target = CurrentPosition() + offset;
                GoTo(target, velocity);
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
