// <copyright file="RemoteObjectBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;

    /// <summary>
    /// Points to a Memory location and reads/understands all the data from there.
    /// Data in remote memory location changes w.r.t time or event. Due to this,
    /// each remote memory object requires to implement a time/event based coroutine.
    /// </summary>
    public abstract class RemoteObjectBase
    {
        private IntPtr address;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteObjectBase"/> class.
        /// </summary>
        internal RemoteObjectBase()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteObjectBase"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal RemoteObjectBase(IntPtr address)
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
        protected abstract void UpdateData();

        /// <summary>
        /// Knows how to clean up the object.
        /// </summary>
        protected abstract void CleanUpData();
    }
}