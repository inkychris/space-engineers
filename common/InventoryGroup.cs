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

        public class InventoryGroup
        {
            private List<IMyInventory> inventories;

            public InventoryGroup(List<IMyEntity> entitylist)
            {
                inventories = new List<IMyInventory>();
                foreach (IMyEntity entity in entitylist)
                    inventories.Add(entity.GetInventory());
            }

            public VRage.MyFixedPoint ItemAmount(MyItemType item)
            {
                VRage.MyFixedPoint count = 0;
                foreach (IMyInventory inventory in inventories)
                    count += inventory.GetItemAmount(item);
                return count;
            }

            public VRage.MyFixedPoint AmountCanTransferTo(IMyInventory target, MyItemType item)
            {
                VRage.MyFixedPoint count = 0;
                foreach (IMyInventory inventory in inventories)
                    if (inventory.CanTransferItemTo(target, item))
                        count += inventory.GetItemAmount(item);
                return count;
            }

            public VRage.MyFixedPoint TransferTo(IMyInventory target, MyItemType itemType, MyFixedPoint? amount = null)
            {
                VRage.MyFixedPoint amountToTransfer;
                if (amount == null)
                    amountToTransfer = ItemAmount(itemType);
                else
                    amountToTransfer = (VRage.MyFixedPoint)amount;
                VRage.MyFixedPoint amountTransferred = 0;
                foreach (IMyInventory inventory in inventories)
                {
                    if (amountTransferred >= amountToTransfer)
                        return amountTransferred;
                    var maybeItem = inventory.FindItem(itemType);
                    if (maybeItem == null)
                        continue;
                    var item = (MyInventoryItem)maybeItem;
                    VRage.MyFixedPoint amountBefore = target.GetItemAmount(itemType);
                    inventory.TransferItemTo(target, item, amountToTransfer - amountTransferred);
                    amountTransferred += target.GetItemAmount(itemType) - amountBefore;
                }
                return amountTransferred;
            }

            public VRage.MyFixedPoint TransferTo(InventoryGroup targetGroup, MyItemType itemType, MyFixedPoint? amount = null)
            {
                VRage.MyFixedPoint amountToTransfer;
                if (amount == null)
                    amountToTransfer = ItemAmount(itemType);
                else
                    amountToTransfer = (VRage.MyFixedPoint)amount;
                VRage.MyFixedPoint amountTransferred = 0;
                foreach (IMyInventory target in targetGroup.inventories)
                {
                    if (amountTransferred >= amountToTransfer)
                        return amountTransferred;
                    amountTransferred += TransferTo(target, itemType, amountToTransfer - amountTransferred);
                }
                return amountTransferred;
            }
        }
    }
}
