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
        public class BlockGroup<T> : IEnumerable<T> where T : class
        {
            protected List<T> group;

            public BlockGroup(List<T> blocklist)
            {
                if (blocklist.Count == 0)
                    throw new ArgumentException("Block group is empty!");
                group = blocklist;
            }

            public T First() { return group.First(); }

            public IEnumerator<T> GetEnumerator() { return group.GetEnumerator(); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        public class FunctionalBlockGroup<T> : BlockGroup<T> where T : class, IMyFunctionalBlock
        {
            public FunctionalBlockGroup(List<T> blocklist) : base(blocklist) { }

            public void Enable()
            {
                foreach (T block in group)
                    block.Enabled = true;
            }

            public void Disable()
            {
                foreach (T block in group)
                    block.Enabled = false;
            }
        }
    }
}
