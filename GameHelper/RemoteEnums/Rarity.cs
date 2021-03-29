// <copyright file="Rarity.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteEnums
{
    /// <summary>
    /// Read Rarity.dat file for Rarity to integer mapping.
    /// </summary>
    public enum Rarity
    {
        /// <summary>
        /// Unknown Item/Monster. This is not a real one.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Normal Item/Monster.
        /// </summary>
        Normal,

        /// <summary>
        /// Magic Item/Monster.
        /// </summary>
        Magic,

        /// <summary>
        /// Rare Item/Monster.
        /// </summary>
        Rare,

        /// <summary>
        /// Unique Item/Monster.
        /// </summary>
        Unique,
    }
}
