// <copyright file="Life.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;
    using System.Collections.Generic;
    using GameHelper.Utils;
    using GameOffsets.Objects.Components;
    using ImGuiNET;

    /// <summary>
    /// The <see cref="Life"/> component in the entity.
    /// </summary>
    public class Life : RemoteObjectBase
    {
        /// <summary>
        /// Stores Key to Effect mapping. This cache saves
        /// 2 x N x M read operations where:
        ///     N = total life components in gamehelper memory,
        ///     M = total number of buff those components has.
        /// </summary>
        private Dictionary<ushort, string> keyToEffectNameCache
            = new Dictionary<ushort, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Life"/> class.
        /// </summary>
        /// <param name="address">address of the <see cref="Life"/> component.</param>
        public Life(IntPtr address)
            : base(address, true)
        {
        }

        /// <summary>
        /// Gets the Buffs/Debuffs associated with the entity.
        /// This is not updated anymore once entity dies.
        /// </summary>
        public Dictionary<string, StatusEffectInfo> StatusEffects { get; private set; }
            = new Dictionary<string, StatusEffectInfo>();

        /// <summary>
        /// Gets a value indicating whether the entity is alive or not.
        /// </summary>
        public bool IsAlive { get; private set; } = true;

        /// <summary>
        /// Gets the health related information of the entity.
        /// </summary>
        public VitalStruct Health { get; private set; } = default;

        /// <summary>
        /// Gets the energyshield related information of the entity.
        /// </summary>
        public VitalStruct EnergyShield { get; private set; } = default;

        /// <summary>
        /// Gets the mana related information of the entity.
        /// </summary>
        public VitalStruct Mana { get; private set; } = default;

        /// <summary>
        /// Converts the <see cref="Life"/> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            if (ImGui.TreeNode("Status Effect (Buff/Debuff) (Click Effect to copy its name)"))
            {
                foreach (var kv in this.StatusEffects)
                {
                    UiHelper.DisplayTextAndCopyOnClick(
                        $"Name: {kv.Key} Details: {kv.Value}", kv.Key);
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Health"))
            {
                this.VitalToImGui(this.Health);
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Energy Shield"))
            {
                this.VitalToImGui(this.EnergyShield);
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Mana"))
            {
                this.VitalToImGui(this.Mana);
                ImGui.TreePop();
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            throw new Exception("Component Address should never be Zero.");
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<LifeOffset>(this.Address);
            this.Health = data.Health;
            this.EnergyShield = data.EnergyShield;
            this.Mana = data.Mana;
            this.IsAlive = data.Health.Current > 0;
            if (this.IsAlive)
            {
                this.StatusEffects.Clear();
                var statusEffects = reader.ReadStdVector<IntPtr>(data.StatusEffectPtr);
                for (int i = 0; i < statusEffects.Length; i++)
                {
                    var kv = reader.ReadMemory<StatusEffectKeyValueStruct>(statusEffects[i]);
                    if (kv.Value == IntPtr.Zero)
                    {
                        continue;
                    }

                    var statusEffectData = reader.ReadMemory<StatusEffectStruct>(kv.Value);
                    var effectInfo = new StatusEffectInfo()
                    {
                        Key = kv.key,
                        TotalTime = statusEffectData.TotalTime,
                        TimeLeft = statusEffectData.TimeLeft,
                        Charges = statusEffectData.Charges,
                    };

                    if (this.keyToEffectNameCache.TryGetValue(kv.key, out var oldEffectname))
                    {
                        // existing Effect
                        this.StatusEffects[oldEffectname] = effectInfo;
                    }
                    else if (this.TryGetNameFromBuffDefination(
                        statusEffectData.BuffDefinationPtr,
                        out var newEffectName))
                    {
                        // New Effect.
                        this.StatusEffects[newEffectName] = effectInfo;
                        this.keyToEffectNameCache[kv.key] = newEffectName;
                    }
                }
            }
            else
            {
                this.StatusEffects.Clear();
                this.keyToEffectNameCache.Clear();
            }
        }

        private void VitalToImGui(VitalStruct data)
        {
            UiHelper.IntPtrToImGui("PtrToSelf", data.PtrToLifeComponent);
            ImGui.Text($"Regeneration: {data.Regeneration}");
            ImGui.Text($"Total: {data.Total}");
            ImGui.Text($"ReservedFlat: {data.ReservedFlat}");
            ImGui.Text($"Current: {data.Current}");
            ImGui.Text($"Reserved(%%): {data.ReservedPercent}");
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

        /// <summary>
        /// Information related to the StatusEffect.
        /// </summary>
        public struct StatusEffectInfo
        {
            /// <summary>
            /// Key associated with the Effect. This is just a counter per buff, per entity, per zone.
            /// This is useful to cache the name of the Effect since 1 key can only associate
            /// with 1 Effect on that entity at any given time.
            /// </summary>
            public ushort Key;

            /// <summary>
            /// Total time the Effect will be valid for.
            /// In case of permanent effects, the value would be +Infinite.
            /// </summary>
            public float TotalTime;

            /// <summary>
            /// Time left for the Effect.
            /// In case of permanent effects, the value would be +Infinite.
            /// </summary>
            public float TimeLeft;

            /// <summary>
            /// Number of charges in this Effect.
            /// </summary>
            public int Charges;

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"Key: {this.Key}, MaxTime: {this.TotalTime}, " +
                    $"TimeLeft: {this.TimeLeft}, Charges: {this.Charges}";
            }
        }
    }
}
