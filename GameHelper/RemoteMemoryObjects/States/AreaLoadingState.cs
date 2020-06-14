// <copyright file="AreaLoadingState.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects.States
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameOffsets.RemoteMemoryObjects.States;

    /// <summary>
    /// Reads AreaLoadingState Game Object.
    /// </summary>
    public sealed class AreaLoadingState : RemoteMemoryObjectBase
    {
        private AreaLoadingStateOffset classData = default;

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaLoadingState"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        public AreaLoadingState(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.GatherData());
        }

        /// <summary>
        /// Gets the Area Changed event.
        /// </summary>
        public Event AreaChanged { get; private set; } = new Event();

        /// <summary>
        /// Gets the game current Area Name.
        /// </summary>
        public string CurrentAreaName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the game is in loading screen or not.
        /// </summary>
        public bool IsLoading => this.classData.IsLoading == 0x01;

        /// <summary>
        /// Throws the Area changed event on demand.
        /// WARNING: This may cause infinite loop if thrown from a function
        /// which is called on an Area change in the first place.
        /// </summary>
        public void ForceThrowAreaChangeEvent()
        {
            CoroutineHandler.RaiseEvent(this.AreaChanged);
        }

        /// <inheritdoc/>
        protected override IEnumerator<Wait> GatherData()
        {
            while (true)
            {
                yield return new Wait(0.5);
                if (this.Address != IntPtr.Zero)
                {
                    var reader = Core.Process.Handle;
                    var data = reader.ReadMemory<AreaLoadingStateOffset>(this.Address);
                    bool hasAreaChanged = false;
                    if (data.CurrentAreaName.Buffer != IntPtr.Zero &&
                        data.IsLoading != 0x01 &&
                        data.TotalLoadingScreenTimeMs > this.classData.TotalLoadingScreenTimeMs)
                    {
                        string areaName = reader.ReadStdWString(data.CurrentAreaName);
                        this.CurrentAreaName = areaName;
                        hasAreaChanged = true;
                    }

                    this.classData = data;
                    if (hasAreaChanged)
                    {
                        CoroutineHandler.RaiseEvent(this.AreaChanged);
                    }
                }
            }
        }
    }
}
