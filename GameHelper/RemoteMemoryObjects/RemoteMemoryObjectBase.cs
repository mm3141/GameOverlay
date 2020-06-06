// <copyright file="RemoteMemoryObjectBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;

    /// <summary>
    /// Points to a Memory location and reads/understands all the data from there.
    /// Data in remote memory location changes w.r.t time or event. Due to this,
    /// each remote memory object requires to implement a time/event based coroutine.
    /// </summary>
    public abstract class RemoteMemoryObjectBase
    {
        private IntPtr address;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteMemoryObjectBase"/> class.
        /// </summary>
        public RemoteMemoryObjectBase()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteMemoryObjectBase"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        public RemoteMemoryObjectBase(IntPtr address)
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
        /// Reads the memory and gather all the data known by this
        /// Object.
        /// </summary>
        /// <returns>co-routine IWait.</returns>
        protected abstract IEnumerator<IWait> GatherData();
    }
}