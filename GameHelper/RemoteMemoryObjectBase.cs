// <copyright file="RemoteMemoryObjectBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper
{
    using System;

    /// <summary>
    /// Points to a Memory location and reads/understands all the data from there.
    /// Data in remote memory location changes w.r.t time or event. Due to this,
    /// each remote memory object exposes (and is base on (time/event based)) coroutines.
    /// </summary>
    public abstract class RemoteMemoryObjectBase
    {
        private IntPtr address;

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
    }
}