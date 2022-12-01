// <copyright file="AilmentCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions
{
    using System.Linq;
    using AutoHotKeyTrigger.ProfileManager.Component;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;

    /// <summary>
    ///     For triggering an action on player Status Effect changes.
    /// </summary>
    public class AilmentCondition : ICondition
    {
        private static readonly AilmentCondition ConfigurationInstance = new("");

        [JsonProperty] private string statusEffectGroupKey;
        [JsonProperty] private IComponent component;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AilmentCondition" /> class.
        /// </summary>
        /// <param name="statusEffectGroupKey">
        ///     Ailment name to look for. This has to be present in
        ///     <see cref="JsonDataHelper.StatusEffectGroups" />.
        /// </param>
        public AilmentCondition(string statusEffectGroupKey)
        {
            this.statusEffectGroupKey = statusEffectGroupKey;
            this.component = null;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public static AilmentCondition Add()
        {
            ConfigurationInstance.ToImGui();
            ImGui.SameLine();
            if (ImGui.Button("Add##StatusEffect") &&
                !string.IsNullOrEmpty(ConfigurationInstance.statusEffectGroupKey))
            {
                return new AilmentCondition(ConfigurationInstance.statusEffectGroupKey);
            }

            return null;
        }

        /// <inheritdoc/>
        public void Add(IComponent component)
        {
            this.component = component;
        }

        /// <inheritdoc />
        public void Display(bool expand)
        {
            this.ToImGui(expand);
            this.component?.Display(expand);
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            var isConditionValid = false;
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (JsonDataHelper.StatusEffectGroups.TryGetValue(this.statusEffectGroupKey, out var statusEffects))
            {
                if (player.GetComp<Buffs>(out var buffComponent))
                {
                    if (statusEffects.Any(statusEffect => buffComponent.StatusEffects.ContainsKey(statusEffect)))
                    {
                        isConditionValid = true;
                    }
                }
            }

            return this.component == null ? isConditionValid : this.component.execute(isConditionValid);
        }

        private void ToImGui(bool expand = true)
        {
            ImGui.Text("Player has");
            ImGui.SameLine();
            if (expand)
            {
                ImGuiHelper.IEnumerableComboBox(
                    "ailment.##AilmentCondition",
                    JsonDataHelper.StatusEffectGroups.Keys,
                    ref this.statusEffectGroupKey);
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.statusEffectGroupKey}");
            }
        }
    }
}