// <copyright file="InGameState.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects.States
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteMemoryObjects.States.InGameStateObjects;
    using GameOffsets.RemoteMemoryObjects.States;

    /// <summary>
    /// Reads InGameState Game Object.
    /// </summary>
    public class InGameState : RemoteMemoryObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InGameState"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal InGameState(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnTick());
        }

        /// <summary>
        /// Gets the InGameStateData.
        /// </summary>
        public InGameStateData Data
        {
            get;
            private set;
        }

        = new InGameStateData(IntPtr.Zero);

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.Data.Address = IntPtr.Zero;
        }

        /// <inheritdoc/>
        protected override void GatherData()
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<InGameStateOffset>(this.Address);
            this.Data.Address = data.LocalData;
        }

        private IEnumerator<Wait> OnTick()
        {
            while (true)
            {
                yield return new Wait(0.5);
                if (this.Address != IntPtr.Zero
                    && Core.States.CurrentStateInGame.Name == GameStateTypes.InGameState)
                {
                    this.GatherData();
                }
                else
                {
                    this.CleanUpData();
                }
            }
        }
    }
}
