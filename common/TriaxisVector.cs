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
        public class TriaxisVector
        {
            public float x, y, z = 0;

            public TriaxisVector() { }
            public TriaxisVector(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public static TriaxisVector operator +(TriaxisVector first, TriaxisVector second)
            {
                return new TriaxisVector(
                    first.x + second.x,
                    first.y + second.y,
                    first.z + second.z
                );
            }

            public static TriaxisVector operator -(TriaxisVector first, TriaxisVector second)
            {
                return new TriaxisVector(
                    first.x - second.x,
                    first.y - second.y,
                    first.z - second.z
                );
            }

            public static TriaxisVector operator *(TriaxisVector first, float scalar)
            {
                return new TriaxisVector(
                    first.x * scalar,
                    first.y * scalar,
                    first.z * scalar
                );
            }

            new public string ToString()
            {
                return string.Format("x:{0} y:{1} z:{2}", x, y, z);
            }
        }
    }
}
