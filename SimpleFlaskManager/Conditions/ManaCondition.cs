// <copyright file="ManaCondition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.Conditions
{
    using System;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;

    /// <summary>
    /// FlaskManager condition to trigger flask on Mana changes.
    /// </summary>
    public class ManaCondition : BaseCondition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManaCondition"/> class.
        /// </summary>
        /// <param name="op">Operator to perform on the ManaCondition.</param>
        /// <param name="threshold">threshold of ManaCondition.</param>
        public ManaCondition(OperatorEnum op, int threshold)
        : base()
        {
            this.Operator = op;
            this.Value = threshold;
        }

        /// <summary>
        /// Gets or sets the value with witch Mana should be compared with.
        /// </summary>
        public int Value { get; set; } = default;

        /// <inheritdoc/>
        public override bool Evaluate()
        {
            var player = Core.States.InGameStateObject.CurrentAreaInstance.Player;
            if (player.TryGetComponent<Life>(out var lifeComponent))
            {
                return this.Operator switch
                {
                    OperatorEnum.BIGGER => lifeComponent.Mana.Current > this.Value && this.EvaluateNext(),
                    OperatorEnum.LESS => lifeComponent.Mana.Current < this.Value && this.EvaluateNext(),
                    _ => throw new Exception($"ManaCondition doesn't support {this.Operator}."),
                };
            }

            return false;
        }
    }
}
