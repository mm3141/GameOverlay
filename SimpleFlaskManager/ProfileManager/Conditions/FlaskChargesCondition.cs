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

    /// <summary>
    /// For triggering a flask on number of flask charges the flask got.
    /// </summary>
    public class FlaskChargesCondition
        : BaseCondition<int>
    {
        private static OperatorEnum operatorStatic = OperatorEnum.BIGGER_THAN;
        private static int flaskSlotStatic = 1;
        private static int chargesStatic = 10;

        [JsonProperty]
        private int slot = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlaskChargesCondition"/> class.
        /// </summary>
        /// <param name="operator_"><see cref="OperatorEnum"/> to use in this condition.</param>
        /// <param name="flaskSlot">Flask slot number who's charges to look for.</param>
        /// <param name="charges">Flask charges threshold to use.</param>
        public FlaskChargesCondition(OperatorEnum operator_, int flaskSlot, int charges)
            : base(operator_, charges)
        {
            this.slot = flaskSlot;
        }

        /// <summary>
        /// Draws the ImGui widget for adding the condition.
        /// </summary>
        /// <returns>
        /// <see cref="ICondition"/> if user wants to add the condition, otherwise null.
        /// </returns>
        public static new FlaskChargesCondition Add()
        {
            ToImGui(ref operatorStatic, ref flaskSlotStatic, ref chargesStatic);
            ImGui.SameLine();
            if (ImGui.Button($"Add##FlaskCharges") &&
                (operatorStatic == OperatorEnum.BIGGER_THAN ||
                operatorStatic == OperatorEnum.LESS_THAN))
            {
                return new FlaskChargesCondition(operatorStatic, flaskSlotStatic, chargesStatic);
            }

            return null;
        }

        /// <inheritdoc/>
        public override void Display(int index = 0)
        {
            ToImGui(ref this.conditionOperator, ref this.slot, ref this.rightHandOperand);
            base.Display(index);
        }

        /// <inheritdoc/>
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
                    OperatorEnum.BIGGER_THAN => chargesComponent.Current > this.rightHandOperand,
                    OperatorEnum.LESS_THAN => chargesComponent.Current < this.rightHandOperand,
                    _ => throw new Exception($"FlaskChargesCondition doesn't support {this.conditionOperator}."),
                }

                && this.EvaluateNext();
            }

            return false;
        }

        private static void ToImGui(ref OperatorEnum operator_, ref int flaskSlot, ref int charges)
        {
            ImGui.Text($"Only {OperatorEnum.BIGGER_THAN} & {OperatorEnum.LESS_THAN} supported.");
            ImGui.Text($"Flask");
            ImGui.SameLine();
            ImGui.DragInt("has##FlaskChargesFlaskSlot", ref flaskSlot, 0.05f, 1, 5);
            ImGui.SameLine();
            UiHelper.EnumComboBox("##FlaskChargesOperator", ref operator_);
            ImGui.SameLine();
            ImGui.DragInt("charges##FlaskChargesFlaskCharge", ref charges, 0.1f, 2, 80);
        }
    }
}
