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
            /// Condition based on player Mana.
            /// </summary>
            MANA,

            /// <summary>
            /// Condition based on player Mana.
            /// </summary>
            MANA_PERCENT,

            /// <summary>
            /// Condition based on player Life.
            /// </summary>
            LIFE,

            /// <summary>
            /// Condition based on player Life.
            /// </summary>
            LIFE_PERCENT,

            /// <summary>
            /// Condition based on player Energy Shield.
            /// </summary>
            ES,

            /// <summary>
            /// Condition based on player Energy Shield.
            /// </summary>
            ES_PERCENT,

            /// <summary>
            /// Conditions based on player Buffs/Debuffs.
            /// </summary>
            MOD,

            /// <summary>
            /// Conditions based on what player is doing.
            /// </summary>
            Animation,
        }

        /// <summary>
        /// Converts the <see cref="ConditionEnum"/> to appropriate ConditionObject.
        /// </summary>
        /// <param name="conditionType">Condition type to create.</param>
        /// <returns>
        /// Returns appropriate condition object in BaseCondition format.
        /// Throws an exception in case it doesn't know how to create a specific Condition.
        /// </returns>
        public static BaseCondition EnumToObject(ConditionEnum conditionType)
        {
            return conditionType switch
            {
                ConditionEnum.MANA => ManaCondition.AddConditionImGuiWidget(),
                ConditionEnum.LIFE => LifeCondition.AddConditionImGuiWidget(),
                ConditionEnum.ES => EnergyShieldCondition.AddConditionImGuiWidget(),
                _ => throw new Exception($"{conditionType} not implemented in ConditionHelper class"),
            };
        }
    }
}
