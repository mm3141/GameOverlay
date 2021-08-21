// <copyright file="Inventory.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Coroutine;
    using GameOffsets.Natives;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;

    /// <summary>
    /// Knows how to parse player, NPC, Crafting, stash inventories available in ServerData
    /// and get the items available in them.
    /// </summary>
    public class Inventory : RemoteObjectBase
    {
        /// <summary>
        /// This array stores items addresses in a given inventory.
        /// Items addresses are in order w.r.t inventory slots. There might be duplicates or IntPtr.Zero
        /// in case an item holds 2 slots or there is no item in the slot respectively.
        /// </summary>
        private IntPtr[] itemsToInventorySlotMapping = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal Inventory(IntPtr address)
            : base(address)
        {
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                this.OnTimeTick(), "[Inventory] Update ServerData Inventory", int.MaxValue - 4));
        }

        /// <summary>
        /// Gets a value indicating total number of boxes in the inventory.
        /// </summary>
        public StdTuple2D<int> TotalBoxes { get; private set; } = default;

        /// <summary>
        /// Gets a value indicating total number of requests send to the server for this inventory.
        /// </summary>
        public int ServerRequestCounter { get; private set; } = default;

        /// <summary>
        /// Gets all the items in the inventory.
        /// </summary>
        public ConcurrentDictionary<IntPtr, Item> Items { get; private set; } =
            new ConcurrentDictionary<IntPtr, Item>();

        /// <summary>
        /// Gets the item at the specific slot in the inventory.
        /// Always check if the returned item IsValid or not.
        /// </summary>
        /// <param name="y">Inventory slot row, starting from 0.</param>
        /// <param name="x">Inventory slot column, starting from 0.</param>
        /// <returns>Item on the given slot.</returns>
        public Item this[int y, int x]
        {
            get
            {
                if (y >= this.TotalBoxes.Y || x >= this.TotalBoxes.X)
                {
                    return new Item(IntPtr.Zero);
                }

                int index = (y * this.TotalBoxes.X) + x;
                IntPtr itemAddr = this.itemsToInventorySlotMapping[index];
                if (itemAddr == IntPtr.Zero)
                {
                    return new Item(IntPtr.Zero);
                }

                if (this.Items.TryGetValue(itemAddr, out var item))
                {
                    return item;
                }

                return new Item(IntPtr.Zero);
            }
        }

        /// <inheritdoc/>
        internal override void ToImGui()
        {
            base.ToImGui();
            ImGui.Text($"Total Boxes: {this.TotalBoxes}");
            ImGui.Text($"Server Request Counter: {this.ServerRequestCounter}");
            if (ImGui.TreeNode("Inventory Slots"))
            {
                for (int y = 0; y < this.TotalBoxes.Y; y++)
                {
                    var data = string.Empty;
                    for (int x = 0; x < this.TotalBoxes.X; x++)
                    {
                        if (this.itemsToInventorySlotMapping[(y * this.TotalBoxes.X) + x] != IntPtr.Zero)
                        {
                            data += " 1";
                        }
                        else
                        {
                            data += " 0";
                        }
                    }

                    ImGui.Text(data);
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Items"))
            {
                foreach (var item in this.Items)
                {
                    if (ImGui.TreeNode($"{item.Value.Path}##{item.Value.Address.ToInt64()}"))
                    {
                        item.Value.ToImGui();
                        ImGui.TreePop();
                    }
                }

                ImGui.TreePop();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.TotalBoxes = default;
            this.ServerRequestCounter = default;
            this.itemsToInventorySlotMapping = null;
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var invInfo = reader.ReadMemory<InventoryStruct>(this.Address);
            this.TotalBoxes = invInfo.TotalBoxes;
            this.ServerRequestCounter = invInfo.ServerRequestCounter;
            this.itemsToInventorySlotMapping = reader.ReadStdVector<IntPtr>(invInfo.ItemList);
            if (hasAddressChanged)
            {
                this.Items.Clear();
            }

            foreach (var item in this.Items)
            {
                if (!item.Value.IsValid)
                {
                    this.Items.TryRemove(item.Key, out _);
                    continue;
                }

                item.Value.IsValid = false;
            }

            Parallel.ForEach(this.itemsToInventorySlotMapping.Distinct(), (invItemPtr) =>
            {
                if (invItemPtr != IntPtr.Zero)
                {
                    var invItem = reader.ReadMemory<InventoryItemStruct>(invItemPtr);
                    if (this.Items.ContainsKey(invItemPtr))
                    {
                        this.Items[invItemPtr].Address = invItem.Item;
                        this.Items[invItemPtr].IsValid = true;
                    }
                    else
                    {
                        var item = new Item(invItem.Item);
                        if (!string.IsNullOrEmpty(item.Path))
                        {
                            if (!this.Items.TryAdd(invItemPtr, item))
                            {
                                throw new Exception("Failed to add item into the Inventory Item Dict.");
                            }
                        }
                    }
                }
            });
        }

        private IEnumerable<Wait> OnTimeTick()
        {
            while (true)
            {
                yield return new Wait(0.02d);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData(false);
                }
            }
        }
    }
}
