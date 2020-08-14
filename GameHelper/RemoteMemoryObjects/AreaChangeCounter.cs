// <copyright file="AreaChangeCounter.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;

    /// <summary>
    /// Points to the AreaChangeCounter object and read/cache it's value
    /// on every area change.
    /// </summary>
    internal class AreaChangeCounter : RemoteMemoryObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AreaChangeCounter"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal AreaChangeCounter(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.GatherData());
        }

        /// <summary>
        /// Gets the cached value of the AreaChangeCounter.
        /// </summary>
        internal int Value { get; private set; } = 0x00;

        /// <inheritdoc/>
        protected override IEnumerator<Wait> GatherData()
        {
            while (true)
            {
                yield return new Wait(Core.States.AreaLoading.AreaChanged);
                if (this.Address != IntPtr.Zero)
                {
                    var reader = Core.Process.Handle;
                    this.Value = reader.ReadMemory<int>(this.Address);
                }
            }
        }
    }
}
