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
            /// Condition based on what player is doing.
            /// </summary>
            Animation,

            /// <summary>
            /// Condition based on player Buffs/Debuffs.
            /// </summary>
            MOD,

            /// <summary>
            /// Condition based on flask mod active on player or not.
            /// </summary>
            FLASK_MOD,

            /// <summary>
            /// Condition based on number of flask Uses.
            /// </summary>
            FLASK_USES,
        }

        /// <summary>
        /// Converts the <see cref="ConditionEnum"/> to the appropriate
        /// <see cref="BaseCondition"/> derived class. This is a wrapper function
        /// that uses the derived classes static function to help create the class.
        /// </summary>
        /// <param name="conditionType">Condition type to create.</param>
        /// <returns>
        /// Returns appropriate condition object in BaseCondition format or null.
        /// Throws an exception in case it doesn't know how to create a specific Condition.
        /// </returns>
        public static BaseCondition EnumToObject(ConditionEnum conditionType)
        {
            return conditionType switch
            {
                ConditionEnum.MANA => ManaCondition.AddConditionImGuiWidget(),
                ConditionEnum.MANA_PERCENT => ManaPercentCondition.AddConditionImGuiWidget(),
                ConditionEnum.LIFE => LifeCondition.AddConditionImGuiWidget(),
                ConditionEnum.LIFE_PERCENT => LifePercentCondition.AddConditionImGuiWidget(),
                ConditionEnum.ES => EnergyShieldCondition.AddConditionImGuiWidget(),
                ConditionEnum.ES_PERCENT => EnergyShieldPercentCondition.AddConditionImGuiWidget(),
                ConditionEnum.Animation => AnimationCondition.AddConditionImGuiWidget(),
                _ => throw new Exception($"{conditionType} not implemented in ConditionHelper class"),
            };
        }
    }
}
