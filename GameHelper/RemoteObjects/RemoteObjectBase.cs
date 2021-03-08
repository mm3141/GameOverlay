// <copyright file="RemoteObjectBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;

    /// <summary>
    /// Points to a Memory location and reads/understands all the data from there.
    /// CurrentAreaInstance in remote memory location changes w.r.t time or event. Due to this,
    /// each remote memory object requires to implement a time/event based coroutine.
    /// </summary>
    public abstract class RemoteObjectBase
    {
        private IntPtr address;
        private bool forceUpdate;

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
        /// <param name="forceUpdate">
        /// True in case the object should be updated even if address hasn't changed.
        /// </param>
        internal RemoteObjectBase(IntPtr address, bool forceUpdate = false)
        {
            this.forceUpdate = forceUpdate;
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
                bool hasAddressChanged = this.address != value;
                if (hasAddressChanged || this.forceUpdate)
                {
                    this.address = value;
                    if (value == IntPtr.Zero)
                    {
                        this.CleanUpData();
                    }
                    else
                    {
                        this.UpdateData(hasAddressChanged);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the memory and update all the data known by this Object.
        /// </summary>
        /// <param name="hasAddressChanged">true in case the address has changed; otherwise false.</param>
        protected abstract void UpdateData(bool hasAddressChanged);

        /// <summary>
        /// Knows how to clean up the object.
        /// </summary>
        protected abstract void CleanUpData();
    }
}