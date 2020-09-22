// <copyright file="InGameStateData.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteEnums;
    using GameOffsets.RemoteMemoryObjects.States.InGameStateObjects;

    /// <summary>
    /// Points to the InGameState -> LocalData Object.
    /// </summary>
    public class InGameStateData : RemoteMemoryObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InGameStateData"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal InGameStateData(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnAreaChange());
            CoroutineHandler.Start(this.OnGameStateChange());
        }

        /// <summary>
        /// Gets the Monster Level of current Area.
        /// </summary>
        public int MonsterLevel { get; private set; } = 0x00;

        /// <summary>
        /// Gets the Hash of the current Area/Zone.
        /// This value is sent to the client from the server.
        /// </summary>
        public string AreaHash { get; private set; } = string.Empty;

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.MonsterLevel = 0x00;
            this.AreaHash = string.Empty;
        }

        /// <inheritdoc/>
        protected override void GatherData()
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<InGameStateDataOffsets>(this.Address);
            this.MonsterLevel = data.MonsterLevel;
            this.AreaHash = $"{data.CurrentAreaHash:X}";
        }

        private IEnumerator<Wait> OnAreaChange()
        {
            yield return new Wait(0);
            while (true)
            {
                yield return new Wait(Core.States.AreaLoading.AreaChangeDetected);
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
                if (Core.States.CurrentStateInGame.Name != GameStateTypes.InGameState
                    && Core.States.CurrentStateInGame.Name != GameStateTypes.EscapeState
                    && Core.States.CurrentStateInGame.Name != GameStateTypes.AreaLoadingState)
                {
                    this.CleanUpData();
                }
            }
        }
    }
}
