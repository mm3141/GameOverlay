// <copyright file="FlaskChargesCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using System.Linq;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using Enums;

    /// <summary>
    ///     For triggering a flask on number of flask charges the flask got.
    /// </summary>
    public class FlaskChargesCondition : ICondition
    {
        private static readonly OperatorType[] SupportedOperatorTypes = { OperatorType.BIGGER_THAN, OperatorType.LESS_THAN };
        private static readonly FlaskChargesCondition ConfigurationInstance = new(OperatorType.BIGGER_THAN, 1, 10);

        [JsonProperty] private OperatorType @operator;
        [JsonProperty] private int flaskSlot;
        [JsonProperty] private int charges;

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
        public void Display()
        {
            this.ToImGui();
        }

        /// <inheritdoc />
        public bool Evaluate()
        {
            var flask = Core.States.InGameStateObject.CurrentAreaInstance.ServerDataObject.FlaskInventory[0, this.flaskSlot - 1];
            if (flask.Address == IntPtr.Zero)
            {
                return false;
            }

            if (flask.TryGetComponent<Charges>(out var chargesComponent))
            {
                return this.@operator switch
                       {
                           OperatorType.BIGGER_THAN => chargesComponent.Current > this.charges,
                           OperatorType.LESS_THAN => chargesComponent.Current < this.charges,
                           _ => throw new Exception($"FlaskChargesCondition doesn't support {this.@operator}.")
                       };
            }

            return false;
        }

        private void ToImGui()
        {
            ImGui.Text("Flask");
            ImGui.SameLine();
            ImGui.DragInt("has##FlaskChargesFlaskSlot", ref this.flaskSlot, 0.05f, 1, 5);
            ImGui.SameLine();
            ImGuiHelper.EnumComboBox("##FlaskChargesOperator", ref this.@operator, SupportedOperatorTypes);
            ImGui.SameLine();
            ImGui.DragInt("charges##FlaskChargesFlaskCharge", ref this.charges, 0.1f, 2, 80);
        }
    }
}