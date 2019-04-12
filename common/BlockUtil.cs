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
    }
}
