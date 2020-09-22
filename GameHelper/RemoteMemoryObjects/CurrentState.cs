// <copyright file="CurrentState.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteMemoryObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteEnums;
    using GameHelper.Utils;
    using GameOffsets.Controllers;
    using GameOffsets.Native;

    /// <summary>
    /// Points to the current state information object in
    /// the game and cache it's value.
    /// </summary>
    internal class CurrentState : RemoteMemoryObjectBase
    {
        private IntPtr currentStateAddress = IntPtr.Zero;
        private GameStateTypes name = GameStateTypes.GameNotLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentState"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal CurrentState(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnTick());
        }

        /// <summary>
        /// Gets the Current State Changed event.
        /// </summary>
        internal Event StateChanged { get; private set; } = new Event();

        /// <summary>
        /// Gets the current state name.
        /// </summary>
        internal GameStateTypes Name
        {
            get => this.name;
            private set
            {
                if (this.name != value)
                {
                    this.name = value;
                    CoroutineHandler.RaiseEvent(this.StateChanged);
                }
            }
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            this.currentStateAddress = IntPtr.Zero;
            this.Name = GameStateTypes.GameNotLoaded;
        }

        /// <inheritdoc/>
        protected override void GatherData()
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<GameStateObject>(this.Address);
            IntPtr stateAddress = this.GetSecondLastPtr(reader, data.CurrentStateOffset1);
            if (stateAddress != IntPtr.Zero && stateAddress != this.currentStateAddress)
            {
                this.currentStateAddress = stateAddress;
                foreach (var state in Core.States.AllStates)
                {
                    if (state.Value == stateAddress)
                    {
                        this.name = this.ConvertStringToEnum(state.Key);
                        break;
                    }
                }
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
                else
                {
                    this.CleanUpData();
                }
            }
        }

        private GameStateTypes ConvertStringToEnum(string data)
        {
            return data switch
            {
                nameof(GameStateTypes.GameNotLoaded) => GameStateTypes.GameNotLoaded,
                nameof(GameStateTypes.InGameState) => GameStateTypes.InGameState,
                nameof(GameStateTypes.PreGameState) => GameStateTypes.PreGameState,
                nameof(GameStateTypes.WaitingState) => GameStateTypes.WaitingState,
                nameof(GameStateTypes.SelectCharacterState) => GameStateTypes.SelectCharacterState,
                nameof(GameStateTypes.LoadingState) => GameStateTypes.LoadingState,
                nameof(GameStateTypes.LoginState) => GameStateTypes.LoadingState,
                nameof(GameStateTypes.CreateCharacterState) => GameStateTypes.CreateCharacterState,
                nameof(GameStateTypes.DeleteCharacterState) => GameStateTypes.DeleteCharacterState,
                nameof(GameStateTypes.EscapeState) => GameStateTypes.EscapeState,
                nameof(GameStateTypes.CreditsState) => GameStateTypes.CreditsState,
                nameof(GameStateTypes.AreaLoadingState) => GameStateTypes.AreaLoadingState,
                nameof(GameStateTypes.ChangePasswordState) => GameStateTypes.ChangePasswordState,
                _ => throw new Exception($"New GameStateTypes discovered: {data}"),
            };
        }

        private IntPtr GetSecondLastPtr(SafeMemoryHandle reader, StdVector vector)
        {
            return reader.ReadMemory<IntPtr>(vector.Last - 0x10);
        }
    }
}
