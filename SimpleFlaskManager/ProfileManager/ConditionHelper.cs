// <copyright file="ConditionHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager.ProfileManager
{
    using System;
    using SimpleFlaskManager.ProfileManager.Conditions;

    /// <summary>
    /// A helper class to add new conditions.
    /// </summary>
    public static class ConditionHelper
    {
        /// <summary>
        /// Conditions supported by the flask manager.
        /// </summary>
        public enum ConditionEnum
        {
            /// <summary>
            /// Condition based on player Vitals.
            /// </summary>
            VITALS,

            /// <summary>
            /// Condition based on what player is doing.
            /// </summary>
            ANIMATION,

            /// <summary>
            /// Condition based on player Buffs/Debuffs.
            /// </summary>
            STATUS_EFFECT,

            /// <summary>
            /// Condition based on flask mod active on player or not.
            /// </summary>
            FLASK_EFFECT,

            /// <summary>
            /// Condition based on number of flask Uses.
            /// </summary>
            FLASK_USES,
        }

        /// <summary>
        /// Converts the <see cref="ConditionEnum"/> to the appropriate
        /// <see cref="ICondition"/> object.
        /// </summary>
        /// <param name="conditionType">Condition type to create.</param>
        /// <returns>
        /// Returns <see cref="ICondition"/> if user wants to create it or null.
        /// Throws an exception in case it doesn't know how to create a specific Condition.
        /// </returns>
        public static ICondition EnumToObject(ConditionEnum conditionType)
        {
            return conditionType switch
            {
                ConditionEnum.VITALS => VitalsCondition.Add(),
                ConditionEnum.ANIMATION => AnimationCondition.Add(),
                ConditionEnum.STATUS_EFFECT => StatusEffectCondition.Add(),
                ConditionEnum.FLASK_EFFECT => FlaskEffectCondition.Add(),
                _ => throw new Exception($"{conditionType} not implemented in ConditionHelper class"),
            };
        }
    }
}
