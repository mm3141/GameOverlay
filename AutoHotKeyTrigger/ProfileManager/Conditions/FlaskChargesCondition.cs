// <copyright file="FlaskChargesCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using Enums;
    using AutoHotKeyTrigger.ProfileManager.Component;

    /// <summary>
    ///     For triggering an action on number of flask charges the flask got.
    /// </summary>
    public class FlaskChargesCondition : ICondition
    {
        private static readonly OperatorType[] SupportedOperatorTypes = { OperatorType.BIGGER_THAN, OperatorType.LESS_THAN };
        private static readonly FlaskChargesCondition ConfigurationInstance = new(OperatorType.BIGGER_THAN, 1, 10);

        [JsonProperty] private OperatorType @operator;
        [JsonProperty] private int flaskSlot;
        [JsonProperty] private int charges;
        [JsonProperty] private IComponent component;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FlaskChargesCondition" /> class.
        /// </summary>
        /// <param name="operator"><see cref="OperatorType" /> to use in this condition.</param>
        /// <param name="flaskSlot">Flask slot number who's charges to look for.</param>
        /// <param name="charges">Flask charges threshold to use.</param>
        public FlaskChargesCondition(OperatorType @operator, int flaskSlot, int charges)
        {
            this.@operator = @operator;
            this.flaskSlot = flaskSlot;
            this.charges = charges;
            this.component = null;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public static FlaskChargesCondition Add()
        {
            ConfigurationInstance.ToImGui();
            ImGui.SameLine();
            if (ImGui.Button("Add##FlaskCharges"))
            {
                return new FlaskChargesCondition(
                    ConfigurationInstance.@operator,
                    ConfigurationInstance.flaskSlot,
                    ConfigurationInstance.charges);
            }

            return null;
        }

        /// <inheritdoc />
        public void Display(bool expand)
        {
            this.ToImGui(expand);
            this.component?.Display(expand);
        }

        /// <inheritdoc/>
        public void Add(IComponent component)
        {
            this.component = component;
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            var isConditionValid = false;
            var flask = Core.States.InGameStateObject.CurrentAreaInstance.ServerDataObject.FlaskInventory[0, this.flaskSlot - 1];
            if (flask.Address != IntPtr.Zero && flask.GetComp<Charges>(out var chargesComponent))
            {
                isConditionValid = this.@operator switch
                {
                    OperatorType.BIGGER_THAN => chargesComponent.Current > this.charges,
                    OperatorType.LESS_THAN => chargesComponent.Current < this.charges,
                    _ => throw new Exception($"FlaskChargesCondition doesn't support {this.@operator}.")
                };
            }

            return this.component == null ? isConditionValid : this.component.execute(isConditionValid);
        }

        private void ToImGui(bool expand = true)
        {
            ImGui.Text("Flask");
            ImGui.SameLine();
            if (expand)
            {
                ImGui.DragInt("has##FlaskChargesFlaskSlot", ref this.flaskSlot, 0.05f, 1, 5);
                ImGui.SameLine();
                ImGuiHelper.EnumComboBox("##FlaskChargesOperator", ref this.@operator, SupportedOperatorTypes);
                ImGui.SameLine();
                ImGui.DragInt("charges##FlaskChargesFlaskCharge", ref this.charges, 0.1f, 2, 80);
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.flaskSlot}");
                ImGui.SameLine();
                if (this.@operator == OperatorType.BIGGER_THAN)
                {
                    ImGui.Text("has more than");
                }
                else
                {
                    ImGui.Text("has less than");
                }

                ImGui.SameLine();
                ImGui.TextColored(new System.Numerics.Vector4(255, 255, 0, 255), $"{this.charges}");
                ImGui.SameLine();
                ImGui.Text("charges");
            }
        }
    }
}