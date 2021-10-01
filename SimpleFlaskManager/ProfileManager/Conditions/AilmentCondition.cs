// <copyright file="AilmentCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;

    /// <summary>
    /// For triggering a flask on player Status Effect changes.
    /// </summary>
    public class AilmentCondition
        : BaseCondition<string>
    {
        private static string statusEffectGroupKeyStatic = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AilmentCondition"/> class.
        /// </summary>
        /// <param name="statusEffectGroupKey">Ailment name to look for. This has to be present in <see cref="JsonDataHelper.StatusEffectGroups"/>.</param>
        public AilmentCondition(string statusEffectGroupKey)
        : base(OperatorEnum.CONTAINS, statusEffectGroupKey)
        {
        }

        /// <summary>
        /// Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        /// <see cref="ICondition"/> if user wants to add the condition, otherwise null.
        /// </returns>
        public static new AilmentCondition Add()
        {
            ToImGui(ref statusEffectGroupKeyStatic);
            ImGui.SameLine();
            if (ImGui.Button($"Add##StatusEffect") &&
                !string.IsNullOrEmpty(statusEffectGroupKeyStatic))
            {
                return new AilmentCondition(statusEffectGroupKeyStatic);
            }

            return null;
        }

        /// <inheritdoc/>
        public override void Display()
        {
            ToImGui(ref this.rightHandOperand);
            base.Display();
        }

        /// <inheritdoc/>
        public override bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            var isAilmentOnPlayer = false;
            if (JsonDataHelper.StatusEffectGroups.TryGetValue(this.rightHandOperand, out var statusEffects))
            {
                if (player.TryGetComponent<Buffs>(out var buffComponent))
                {
                    for (int i = 0; i < statusEffects.Count; i++)
                    {
                        if (buffComponent.StatusEffects.ContainsKey(statusEffects[i]))
                        {
                            isAilmentOnPlayer = true;
                            break;
                        }
                    }
                }
            }

            return isAilmentOnPlayer && this.EvaluateNext();
        }

        private static void ToImGui(ref string statusEffectGroupKey)
        {
            ImGui.Text($"Player has");
            ImGui.SameLine();
            UiHelper.IEnumerableComboBox(
                "ailment.##AilmentCondition",
                JsonDataHelper.StatusEffectGroups.Keys,
                ref statusEffectGroupKey);
        }
    }
}
