// <copyright file="AreaLoadingState.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects.States
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteEnums;
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
        internal AreaLoadingState(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnTick());
            CoroutineHandler.Start(this.OnGameStateChange());
        }

        /// <summary>
        /// Gets the Area Changed event. Area change is detected by
        /// checking if the time spend on the loading screen is greater
        /// than the last recorded time.
        ///
        /// NOTE:Game provides the value of time spend on the loading screen.
        /// </summary>
        public Event AreaChanged { get; private set; } = new Event();

        /// <summary>
        /// Gets the game current Area Name.
        /// </summary>
        public string CurrentAreaName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the Area Change Detection event. When Area Change happens many
        /// classes want to gather new data from the new Area. Unfortunately, Some of
        /// the those classes have inter-dependicies with each other.
        /// This event is called before AreaChanged event, so this event can
        /// be used to resolve some inter-dependicies. This is required since co-routine
        /// doesn't guarantees a certian order in execution of functions. In the future,
        /// if inter-dependicies gets out of control we might have to ditch co-routines
        /// for AreaChange scenario. So, use this event if your class can independently
        /// GatherData, otherwise use AreaChanged.
        /// </summary>
        internal Event AreaChangeDetected { get; private set; } = new Event();

        /// <summary>
        /// Gets a value indicating whether the game is in loading screen or not.
        /// </summary>
        internal bool IsLoading =>
            this.classData.IsLoading == 0x01;

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.classData = default;
            this.CurrentAreaName = string.Empty;
        }

        /// <inheritdoc/>
        protected override void GatherData()
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
                CoroutineHandler.RaiseEvent(this.AreaChangeDetected);
                CoroutineHandler.RaiseEvent(this.AreaChanged);
            }
        }

        private IEnumerator<Wait> OnTick()
        {
            while (true)
            {
                yield return new Wait(0.5);
                if (this.Address != IntPtr.Zero)
                {
                    this.GatherData();
                }
            }
        }

        private IEnumerator<Wait> OnGameStateChange()
        {
            yield return new Wait(0);
            while (true)
            {
                yield return new Wait(Core.States.CurrentStateInGame.StateChanged);
                if (Core.States.CurrentStateInGame.Name == GameStateTypes.PreGameState
                    || Core.States.CurrentStateInGame.Name == GameStateTypes.GameNotLoaded)
                {
                    this.CleanUpData();
                }
            }
        }
    }
}
