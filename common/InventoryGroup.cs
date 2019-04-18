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

            private InventoryGroup(List<IMyInventory> inventorylist) { inventories = inventorylist; }

            public VRage.MyFixedPoint ItemAmount(MyItemType item)
            {
                VRage.MyFixedPoint count = 0;
                foreach (IMyInventory inventory in inventories)
                    count += inventory.GetItemAmount(item);
                return count;
            }

            public VRage.MyFixedPoint MinAmountInOne(MyItemType item)
            {
                VRage.MyFixedPoint minCount = MyFixedPoint.MaxValue;
                foreach (IMyInventory inventory in inventories)
                    minCount = MyFixedPoint.Min(minCount, inventory.GetItemAmount(item));
                if (minCount == MyFixedPoint.MaxValue)
                    return 0;
                return minCount;
            }

            public bool CanTransferTo(InventoryGroup targetGroup, MyItemType item)
            {
                foreach (IMyInventory inventory in inventories)
                    foreach (IMyInventory target in targetGroup.inventories)
                        if (inventory.CanTransferItemTo(target, item))
                            return true;
                return false;
            }

            public static VRage.MyFixedPoint TransferBetween(IMyInventory source, IMyInventory target, MyItemType itemType, VRage.MyFixedPoint? amount = null)
            {
                if (source == target)
                    return 0;
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                source.GetItems(items, sourceItem => sourceItem.Type == itemType);
                if (items.Count == 0)
                    return 0;
                VRage.MyFixedPoint totalTransferred = 0;
                foreach (MyInventoryItem item in items)
                {
                    VRage.MyFixedPoint amountBefore = target.GetItemAmount(itemType);
                    source.TransferItemTo(target, item, amount - totalTransferred);
                    totalTransferred += target.GetItemAmount(itemType) - amountBefore;
                }
                return totalTransferred;
            }

            public VRage.MyFixedPoint TransferTo(IMyInventory target, MyItemType itemType, MyFixedPoint? amount = null)
            {
                VRage.MyFixedPoint amountToTransfer;
                if (amount == null)
                    amountToTransfer = ItemAmount(itemType);
                else
                    amountToTransfer = ((VRage.MyFixedPoint)amount).ToIntSafe();
                VRage.MyFixedPoint amountTransferred = 0;
                foreach (IMyInventory inventory in inventories)
                {
                    if (amountTransferred >= amountToTransfer)
                        return amountTransferred;
                    amountTransferred += TransferBetween(inventory, target, itemType, amountToTransfer - amountTransferred);
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

            private InventoryGroup Excluding(IMyInventory inventory)
            {
                List<IMyInventory> newList = new List<IMyInventory>();
                foreach (IMyInventory existing in inventories)
                    if (existing != inventory)
                        newList.Add(existing);
                return new InventoryGroup(newList);
            }

            public void Balance(MyItemType item)
            {
                var balancedAmount = VRage.MyFixedPoint.Floor((VRage.MyFixedPoint)((double)ItemAmount(item) / inventories.Count));
                InventoryGroup invGroup = this;
                foreach (IMyInventory inventory in inventories)
                {
                    var currentAmount = inventory.GetItemAmount(item);
                    invGroup = invGroup.Excluding(inventory);
                    if (currentAmount < balancedAmount)
                    {
                        invGroup.TransferTo(inventory, item, balancedAmount - currentAmount);
                        continue;
                    }
                    var extraAmount = currentAmount - balancedAmount;
                    foreach (IMyInventory otherInventory in invGroup.inventories)
                        extraAmount -= TransferBetween(inventory, otherInventory, item, extraAmount);
                }
            }
        }
    }
}
