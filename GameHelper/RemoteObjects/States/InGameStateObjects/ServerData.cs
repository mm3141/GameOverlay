// <copyright file="ServerData.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils;
    using GameOffsets.Objects.States.InGameState;
    using ImGuiNET;

    /// <summary>
    /// Points to the InGameState -> ServerData object.
    /// </summary>
    public class ServerData : RemoteObjectBase
    {
        private InventoryName selectedInvName = InventoryName.NoInvSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerData"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal ServerData(IntPtr address)
            : base(address)
        {
            Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(
                this.OnTimeTick(), "[ServerData] Update ServerData", int.MaxValue - 3));
        }

        /// <summary>
        /// Gets an object that points to the flask inventory.
        /// </summary>
        public Inventory FlaskInventory
        {
            get;
            private set;
        }

        = new Inventory(IntPtr.Zero, "Flask");

        /// <summary>
        /// Gets the inventory to debug.
        /// </summary>
        internal Inventory CurrentlySelectedInventory
        {
            get;
            private set;
        }

        = new Inventory(IntPtr.Zero, "CurrentlySelected");

        /// <summary>
        /// Gets the inventories associated with the player.
        /// </summary>
        internal Dictionary<InventoryName, IntPtr> PlayerInventories
        {
            get;
            private set;
        }

        = new Dictionary<InventoryName, IntPtr>();

        /// <inheritdoc/>
        internal override void ToImGui()
        {
            if (((int)this.selectedInvName) > this.PlayerInventories.Count)
            {
                this.ClearCurrentlySelectedInventory();
            }

            UiHelper.IntPtrToImGui("Address", this.Address);
            if (ImGui.TreeNode("FlaskInventory"))
            {
                this.FlaskInventory.ToImGui();
                ImGui.TreePop();
            }

            ImGui.Text("please click Clear Selected before leaving this window.");
            if (ImGui.BeginCombo("###Inventory Selector", this.selectedInvName.ToString()))
            {
                foreach (var inv in this.PlayerInventories)
                {
                    bool selected = inv.Key == this.selectedInvName;
                    if (ImGui.IsWindowAppearing() && selected)
                    {
                        ImGui.SetScrollHereY();
                    }

                    if (ImGui.Selectable(inv.Key.ToString(), selected))
                    {
                        this.selectedInvName = inv.Key;
                        this.CurrentlySelectedInventory.Address = inv.Value;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear Selected"))
            {
                this.ClearCurrentlySelectedInventory();
            }

            if (this.selectedInvName != InventoryName.NoInvSelected)
            {
                if (ImGui.TreeNode("CurrentlySelectedInventory"))
                {
                    this.CurrentlySelectedInventory.ToImGui();
                    ImGui.TreePop();
                }
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.ClearCurrentlySelectedInventory();
            this.PlayerInventories.Clear();
            this.FlaskInventory.Address = IntPtr.Zero;
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            // only happens when area is changed.
            if (hasAddressChanged)
            {
                this.ClearCurrentlySelectedInventory();
            }

            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<ServerDataStructure>(this.Address + ServerDataStructure.SKIP);
            var inventoryData = reader.ReadStdVector<InventoryArrayStruct>(data.PlayerInventories);
            this.PlayerInventories.Clear();
            for (int i = 0; i < inventoryData.Length; i++)
            {
                InventoryName invName = (InventoryName)inventoryData[i].InventoryId;
                IntPtr invAddr = inventoryData[i].InventoryPtr0;
                this.PlayerInventories[invName] = invAddr;
                switch (invName)
                {
                    case InventoryName.Flask1:
                        this.FlaskInventory.Address = invAddr;
                        break;
                    default:
                        break;
                }
            }
        }

        private void ClearCurrentlySelectedInventory()
        {
            this.selectedInvName = InventoryName.NoInvSelected;
            this.CurrentlySelectedInventory.Address = IntPtr.Zero;
        }

        private IEnumerable<Wait> OnTimeTick()
        {
            while (true)
            {
                yield return new Wait(0.2d);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData(false);
                }
            }
        }
    }
}
