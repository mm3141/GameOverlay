// <copyright file="InventoryPanelUiElement.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.UiElement
{
    using System;
    using System.Collections.Generic;
    using GameHelper.RemoteEnums;
    using GameOffsets.Objects.UiElement;
    using ImGuiNET;

    /// <summary>
    /// Points to the Player Inventory Panel Ui Element.
    /// </summary>
    public class InventoryPanelUiElement : UiElementBase
    {
        private int inventoryIndexToDebug = 0x00;
        private IntPtr[] inventoriesAddresses = new IntPtr[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryPanelUiElement"/> class.
        /// </summary>
        /// <param name="address">address to the Map Ui Element of the game.</param>
        internal InventoryPanelUiElement(IntPtr address)
            : base(address)
        {
        }

        /// <summary>
        /// Gets the player inventory00.
        /// </summary>
        internal UiElementBase DebuggingInventory
        {
            get;
            private set;
        }

        = new UiElementBase(IntPtr.Zero);

        /// <summary>
        /// Get all the items from the given inventory (if the inventory is visible)
        /// and sort it according to inventory position.
        /// </summary>
        /// <param name="inventoryName">Inventory name whos items you want.</param>
        /// <returns>List of items in the given inventory.</returns>
        public List<InventoryItemOffset> GetInventoryVisibleItemsSorted(InventoryName inventoryName)
        {
            List<InventoryItemOffset> data = new List<InventoryItemOffset>();
            int invIndex = (int)inventoryName;
            if (invIndex >= this.inventoriesAddresses.Length)
            {
                return data;
            }

            var invaddr = this.inventoriesAddresses[invIndex];
            if (invaddr == IntPtr.Zero)
            {
                return data;
            }

            var reader = Core.Process.Handle;
            var inventory = reader.ReadMemory<UiElementBaseOffset>(invaddr);
            var invSlots = reader.ReadStdVector<IntPtr>(inventory.ChildrensPtr);
            for (int i = 0; i < invSlots.Length; i++)
            {
                var invItem = reader.ReadMemory<InventoryItemOffset>(invSlots[i]);
                data.Add(invItem);
            }

            data.Sort(
                (x, y) => (
                x.ItemDetails.Position.X + x.ItemDetails.Position.Y).CompareTo(
                    y.ItemDetails.Position.X + y.ItemDetails.Position.Y));
            return data;
        }

        /// <summary>
        /// Converts the <see cref="InventoryPanelUiElement"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            var totalInv = InventoryPanelUiElementOffset.TotalInventories;
            var invName = (InventoryName)this.inventoryIndexToDebug;
            if (ImGui.BeginCombo("Inventory To Debug", $"{invName}", ImGuiComboFlags.PopupAlignLeft))
            {
                for (int i = 0; i < totalInv; i++)
                {
                    bool isSelected = this.inventoryIndexToDebug == i;
                    if (isSelected && ImGui.IsWindowAppearing())
                    {
                        // Not sure why ImGui.SetItemDefaultFocus(); isn't working.
                        // TODO: Update to latest ImGui and test again.
                        ImGui.SetScrollHereY();
                    }

                    if (ImGui.Selectable($"{(InventoryName)i}", isSelected))
                    {
                        this.inventoryIndexToDebug = i;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SliderInt("Inventory Index To Debug", ref this.inventoryIndexToDebug, 0, totalInv - 1);
            var invItems = this.GetInventoryVisibleItemsSorted(invName);
            if (ImGui.BeginCombo("###Debugged Inventory Items", $"Inventory Items", ImGuiComboFlags.PopupAlignLeft))
            {
                for (int i = 0; i < invItems.Count; i++)
                {
                    ImGui.Selectable($"Pos: {invItems[i].ItemDetails.Position}, Size: {invItems[i].ItemDetails.Size}, Addr: {invItems[i].ItemDetails.Item.ToInt64():X}");
                }

                ImGui.EndCombo();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            base.CleanUpData();
            this.inventoryIndexToDebug = 0x00;
            this.DebuggingInventory.Address = IntPtr.Zero;
            this.inventoriesAddresses = new IntPtr[0];
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            base.UpdateData(hasAddressChanged);
            var reader = Core.Process.Handle;
            var invStartPtr = this.Address + InventoryPanelUiElementOffset.InventoryListOffset;
            var totalInv = InventoryPanelUiElementOffset.TotalInventories;
            this.inventoriesAddresses = reader.ReadMemoryArray<IntPtr>(invStartPtr, totalInv);
            if (this.inventoriesAddresses[this.inventoryIndexToDebug] != IntPtr.Zero)
            {
                this.DebuggingInventory.Address = this.inventoriesAddresses[this.inventoryIndexToDebug];
            }
        }
    }
}
