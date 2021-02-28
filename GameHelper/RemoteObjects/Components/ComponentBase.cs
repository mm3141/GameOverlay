// <copyright file="ComponentBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.Components
{
    using System;

    /// <summary>
    /// Component associated with the Entity.
    /// </summary>
    public abstract class ComponentBase
    {
        private IntPtr address;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBase"/> class.
        /// </summary>
        /// <param name="address">address of the component.</param>
        public ComponentBase(IntPtr address)
        {
            this.Address = address;
        }

        /// <summary>
        /// Gets or sets the address of the memory location.
        /// </summary>
        public IntPtr Address
        {
            get => this.address;
            set
            {
                this.address = value;
            }
        }

        /// <summary>
        /// Reads the memory and update all the data known by this Object.
        /// </summary>
        public abstract void UpdateData();
    }
}
