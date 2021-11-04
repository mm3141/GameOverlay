// <copyright file="Buffs.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using System.Collections.Concurrent;
    using GameOffsets.Objects.Components;
    using ImGuiNET;
    using Utils;

    /// <summary>
    ///     The <see cref="Buffs" /> component in the entity.
    /// </summary>
    public class Buffs : RemoteObjectBase
    {
        /// <summary>
        ///     Stores Key to Effect mapping. This cache saves
        ///     2 x N x M read operations where:
        ///     N = total life components in gamehelper memory,
        ///     M = total number of buff those components has.
        /// </summary>
        private static readonly ConcurrentDictionary<IntPtr, string> AddressToEffectNameCache = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Buffs" /> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Buffs" /> component.</param>
        public Buffs(IntPtr address)
            : base(address, true) { }

        /// <summary>
        ///     Gets the Buffs/Debuffs associated with the entity.
        ///     This is not updated anymore once entity dies.
        /// </summary>
        public ConcurrentDictionary<string, StatusEffectStruct> StatusEffects { get; } = new();

        /// <inheritdoc />
        internal override void ToImGui()
        {
            base.ToImGui();
            if (ImGui.TreeNode("Status Effect (Buff/Debuff) (Click Effect to copy its name)"))
            {
                foreach (var kv in this.StatusEffects)
                {
                    ImGuiHelper.DisplayTextAndCopyOnClick($"Name: {kv.Key}", kv.Key);
                    ImGui.SameLine();
                    ImGui.Text($" Details: {kv.Value}");
                }

                ImGui.TreePop();
            }
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            throw new Exception("Component Address should never be Zero.");
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<BuffsOffsets>(this.Address);
            this.StatusEffects.Clear();
            var statusEffects = reader.ReadStdVector<IntPtr>(data.StatusEffectPtr);
            for (var i = 0; i < statusEffects.Length; i++)
            {
                var statusEffectData = reader.ReadMemory<StatusEffectStruct>(statusEffects[i]);
                if (AddressToEffectNameCache.TryGetValue(statusEffectData.BuffDefinationPtr, out var oldEffectname))
                {
                    // known Effect
                    this.StatusEffects.AddOrUpdate(oldEffectname, statusEffectData, (key, oldValue) =>
                    {
                        statusEffectData.Charges = ++oldValue.Charges;
                        return statusEffectData;
                    });
                }
                else if (this.TryGetNameFromBuffDefination(
                    statusEffectData.BuffDefinationPtr,
                    out var newEffectName))
                {
                    // Unknown Effect.
                    this.StatusEffects.AddOrUpdate(newEffectName, statusEffectData, (key, oldValue) =>
                    {
                        statusEffectData.Charges = ++oldValue.Charges;
                        return statusEffectData;
                    });

                    AddressToEffectNameCache[statusEffectData.BuffDefinationPtr] = newEffectName;
                }
            }
        }

        private bool TryGetNameFromBuffDefination(IntPtr addr, out string name)
        {
            var reader = Core.Process.Handle;
            var namePtr = reader.ReadMemory<IntPtr>(addr);
            name = reader.ReadUnicodeString(namePtr);
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return true;
        }
    }
}