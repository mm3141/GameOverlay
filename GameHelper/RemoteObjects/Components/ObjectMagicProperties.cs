// <copyright file="ObjectMagicProperties.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;

    /// <summary>
    /// ObjectMagicProperties component of the entity.
    /// </summary>
    public class ObjectMagicProperties : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectMagicProperties"/> class.
        /// </summary>
        /// <param name="address">address of the <see cref="ObjectMagicProperties"/> component.</param>
        public ObjectMagicProperties(IntPtr address)
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
