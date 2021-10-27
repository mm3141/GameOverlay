// <copyright file="DiesAfterTime.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;

    /// <summary>
    /// The <see cref="DiesAfterTime"/> component in the entity.
    /// </summary>
    public class DiesAfterTime : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiesAfterTime"/> class.
        /// </summary>
        /// <param name="address">address of the <see cref="MinimapIcon"/> component.</param>
        public DiesAfterTime(IntPtr address)
            : base(address, true)
        {
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            throw new Exception("Component Address should never be Zero.");
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
        }
    }
}
