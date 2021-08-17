// <copyright file="Profile.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SimpleFlaskManager
{
    using System;
    using System.Collections.Generic;
    using global::SimpleFlaskManager.Conditions;

    /// <summary>
    /// A class for creating/storing rules on
    /// how/when to drink the flask.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// Gets the rules to trigger the flasks on.
        /// </summary>
        public List<RuleStruct> Rules { get; private set; }
            = new List<RuleStruct>();

        /// <summary>
        /// A struct for storing condition and action.
        /// </summary>
        public struct RuleStruct
        {
            /// <summary>
            /// Rule condition.
            /// </summary>
            public BaseCondition Condition;

            /// <summary>
            /// Rule key to press on success.
            /// </summary>
            public ConsoleKey ActionKey;
        }
    }
}