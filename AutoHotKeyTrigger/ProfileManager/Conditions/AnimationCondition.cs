// <copyright file="AnimationCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions
{
    using System;
    using System.Diagnostics;
    using GameHelper;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using Enums;

    /// <summary>
    ///     For triggering an action on player animation changes.
    /// </summary>
    public class AnimationCondition : ICondition
    {
        private static readonly OperatorType[] SupportedOperatorTypes = { OperatorType.EQUAL_TO, OperatorType.NOT_EQUAL_TO };
        private static readonly AnimationCondition ConfigurationInstance = new(OperatorType.EQUAL_TO, Animation.Idle, 0);

        private readonly Stopwatch sw;

        [JsonProperty] private OperatorType @operator;
        [JsonProperty] private Animation animation;
        [JsonProperty] private int durationMs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnimationCondition" /> class.
        /// </summary>
        /// <param name="operator"><see cref="OperatorType" /> to use in this condition.</param>
        /// <param name="animation">player animation to use for this condition.</param>
        /// <param name="duration">duration (ms) for which the animation is active.</param>
        public AnimationCondition(OperatorType @operator, Animation animation, int duration)
        {
            this.@operator = @operator;
            this.animation = animation;
            this.durationMs = duration;
            this.sw = Stopwatch.StartNew();
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public static AnimationCondition Add()
        {
            ConfigurationInstance.ToImGui();
            ImGui.SameLine();
            if (ImGui.Button("Add##Animation"))
            {
                return new AnimationCondition(
                    ConfigurationInstance.@operator,
                    ConfigurationInstance.animation,
                    ConfigurationInstance.durationMs);
            }

            return null;
        }

        /// <inheritdoc />
        public void Display(bool expand)
        {
            this.ToImGui(expand);
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Actor>(out var actorComponent))
            {
                var isConditionValid = this.@operator switch
                {
                    OperatorType.EQUAL_TO => actorComponent.Animation == this.animation,
                    OperatorType.NOT_EQUAL_TO => actorComponent.Animation != this.animation,
                    _ => throw new Exception($"AnimationCondition doesn't support {this.@operator}.")
                };

                if (isConditionValid)
                {
                    if (this.sw.ElapsedMilliseconds >= this.durationMs)
                    {
                        return true;
                    }
                }
                else
                {
                    this.sw.Restart();
                }
            }

            return false;
        }

        private void ToImGui(bool expand = true)
        {
            ImGui.Text("Player animation is");
            ImGui.SameLine();
            if (expand)
            {
                ImGuiHelper.EnumComboBox("##AnimationOperator", ref this.@operator, SupportedOperatorTypes);
                ImGui.SameLine();
                ImGuiHelper.EnumComboBox("for ##AnimationRHS", ref this.animation);
                ImGui.SameLine();
                ImGui.InputInt("ms##AnimationDuration", ref this.durationMs);
            }
            else
            {
                if (this.@operator != OperatorType.EQUAL_TO)
                {
                    ImGui.Text("not");
                    ImGui.SameLine();
                }

                ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.animation}");
                ImGui.SameLine();
                ImGui.Text("for");
                ImGui.SameLine();
                ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.durationMs}");
                ImGui.SameLine();
                ImGui.Text("(ms)");
            }
        }
    }
}
