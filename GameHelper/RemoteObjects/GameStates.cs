// <copyright file="GameStates.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using CoroutineEvents;
    using GameOffsets.Objects;
    using ImGuiNET;
    using RemoteEnums;
    using States;
    using Utils;

    /// <summary>
    ///     Reads and stores the global states of the game.
    /// </summary>
    public class GameStates : RemoteObjectBase
    {
        private Dictionary<IntPtr, GameStateTypes> gameStateCache = new();
        private GameStateTypes currentStateName = GameStateTypes.GameNotLoaded;
        private GameStateStaticOffset myStaticObj;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameStates" /> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal GameStates(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnPerFrame(), priority: int.MaxValue);
        }

        /// <summary>
        ///     Gets the AreaLoadingState object.
        /// </summary>
        public AreaLoadingState AreaLoading { get; } = new(IntPtr.Zero);

        /// <summary>
        ///     Gets the InGameState Object.
        /// </summary>
        public InGameState InGameStateObject { get; } = new(IntPtr.Zero);

        /// <summary>
        ///     Gets the current state the game is in.
        /// </summary>
        public GameStateTypes GameCurrentState
        {
            get => this.currentStateName;
            private set
            {
                if (this.currentStateName != value)
                {
                    this.currentStateName = value;
                    if (value != GameStateTypes.GameNotLoaded)
                    {
                        CoroutineHandler.RaiseEvent(RemoteEvents.StateChanged);
                    }
                }
            }
        }

        /// <summary>
        ///     Converts the <see cref="GameStates" /> class data to ImGui.
        /// </summary>
        internal override void ToImGui()
        {
            base.ToImGui();
            if (ImGui.TreeNode("All States Info"))
            {
                foreach (var state in this.gameStateCache)
                {
                    ImGuiHelper.IntPtrToImGui($"{state.Value}", state.Key);
                }

                ImGui.TreePop();
            }

            ImGui.Text($"Current State: {this.GameCurrentState}");
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            if (hasAddressChanged)
            {
                this.myStaticObj = reader.ReadMemory<GameStateStaticOffset>(this.Address);
            }

            var data = reader.ReadMemory<GameStateOffset>(this.myStaticObj.GameState);
            var cStateAddr = reader.ReadMemory<IntPtr>(data.CurrentStatePtr.Last - 0x10); // Get 2nd-last ptr.
            this.AddToCacheAndKnownStateObject(cStateAddr);
            this.GameCurrentState = this.gameStateCache[cStateAddr];
        }

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            this.myStaticObj = default;
            this.GameCurrentState = GameStateTypes.GameNotLoaded;
            this.gameStateCache.Clear();
            this.AreaLoading.Address = IntPtr.Zero;
            this.InGameStateObject.Address = IntPtr.Zero;
        }

        /// <summary>
        ///     Updates the state cache and known state object based on state address.
        ///     Cache helps save 1 memory read per frame.
        /// </summary>
        /// <param name="stateptr">state address</param>
        private void AddToCacheAndKnownStateObject(IntPtr stateptr)
        {
            var reader = Core.Process.Handle;
            if (!this.gameStateCache.ContainsKey(stateptr))
            {
                var stateheader = reader.ReadMemory<StateHeaderStruct>(stateptr);
                var statetype = (GameStateTypes)stateheader.GameStateTypeEnum;
                this.gameStateCache[stateptr] = statetype;
                this.UpdateKnownStatesObjects(statetype, stateptr);
            }
        }

        /// <summary>
        ///     Updates the known states Objects and silently skips the unknown ones.
        /// </summary>
        /// <param name="name">State type.</param>
        /// <param name="address">State address.</param>
        private void UpdateKnownStatesObjects(GameStateTypes name, IntPtr address)
        {
            switch (name)
            {
                case GameStateTypes.AreaLoadingState:
                    this.AreaLoading.Address = address;
                    break;
                case GameStateTypes.InGameState:
                    this.InGameStateObject.Address = address;
                    break;
            }
        }

        private IEnumerator<Wait> OnPerFrame()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData(false);
                }
            }
        }
    }
}