// <copyright file="FlaskChargesCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.Utils;
    using ImGuiNET;
    using Newtonsoft.Json;
    using SimpleFlaskManager.ProfileManager.Enums;

    /// <summary>
    ///     For triggering a flask on number of flask charges the flask got.
    /// </summary>
    public class FlaskChargesCondition
        : BaseCondition<int>
    {
        private static OperatorType operatorStatic = OperatorType.BIGGER_THAN;
        private static int flaskSlotStatic = 1;
        private static int chargesStatic = 10;

        [JsonProperty] private int slot = 1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FlaskChargesCondition" /> class.
        /// </summary>
        /// <param name="operator_"><see cref="OperatorType" /> to use in this condition.</param>
        /// <param name="flaskSlot">Flask slot number who's charges to look for.</param>
        /// <param name="charges">Flask charges threshold to use.</param>
        public FlaskChargesCondition(OperatorType operator_, int flaskSlot, int charges) : base(operator_, charges)
        {
            this.slot = flaskSlot;
        }

        /// <summary>
        ///     Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        ///     <see cref="ICondition" /> if user wants to add the condition, otherwise null.
        /// </returns>
        public new static FlaskChargesCondition Add()
        {
            ToImGui(ref operatorStatic, ref flaskSlotStatic, ref chargesStatic);
            ImGui.SameLine();
            if (ImGui.Button("Add##FlaskCharges") &&
                (operatorStatic == OperatorType.BIGGER_THAN || operatorStatic == OperatorType.LESS_THAN))
            {
                return new FlaskChargesCondition(operatorStatic, flaskSlotStatic, chargesStatic);
            }

            return null;
        }

        /// <inheritdoc />
        public override void Display(int index = 0)
        {
            ToImGui(ref this.conditionOperator, ref this.slot, ref this.rightHandOperand);
            base.Display(index);
        }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            var flask = Core.States.InGameStateObject.CurrentAreaInstance.ServerDataObject.FlaskInventory[0, this.slot - 1];
            if (flask.Address == IntPtr.Zero)
            {
                return false;
            }

            if (flask.TryGetComponent<Charges>(out var chargesComponent))
            {
                return this.conditionOperator switch
                       {
                           OperatorType.BIGGER_THAN => chargesComponent.Current > this.rightHandOperand,
                           OperatorType.LESS_THAN => chargesComponent.Current < this.rightHandOperand,
                           _ => throw new Exception($"FlaskChargesCondition doesn't support {this.conditionOperator}.")
                       };
            }

            return false;
        }

        private static void ToImGui(ref OperatorType operator_, ref int flaskSlot, ref int charges)
        {
            ImGui.Text("Flask");
            ImGui.SameLine();
            ImGui.DragInt("has##FlaskChargesFlaskSlot", ref flaskSlot, 0.05f, 1, 5);
            ImGui.SameLine();
            ImGuiHelper.EnumComboBox("##FlaskChargesOperator", ref operator_);
            ImGuiHelper.ToolTip($"Only {OperatorType.BIGGER_THAN} & {OperatorType.LESS_THAN} supported.");
            ImGui.SameLine();
            ImGui.DragInt("charges##FlaskChargesFlaskCharge", ref charges, 0.1f, 2, 80);
        }
    }
}