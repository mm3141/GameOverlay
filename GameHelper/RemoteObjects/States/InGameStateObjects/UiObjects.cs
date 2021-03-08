// <copyright file="UiObjects.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;

    /// <summary>
    /// Points to all the important Ui Elements in the game
    /// and keeps them up to date.
    /// </summary>
    public class UiObjects : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UiObjects"/> class.
        /// </summary>
        /// <param name="address">address of the structure containing all the Ui Objects.</param>
        internal UiObjects(IntPtr address)
            : base(address)
        {
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
        }
    }
}
