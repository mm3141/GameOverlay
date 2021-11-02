// <copyright file="ImportantUiElements.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using CoroutineEvents;
    using GameOffsets.Objects.States.InGameState;
    using RemoteEnums;
    using UiElement;

    /// <summary>
    ///     This is actually UiRoot main child which contains
    ///     all the UiElements (100+). Normally it's at index 1 of UiRoot.
    ///     This class is created because traversing childrens of
    ///     UiRoot is a slow process that requires lots of memory reads.
    ///     Drawback:
    ///     1: Every league/patch the offsets needs to be updated.
    ///     Offsets used over here are very unstable.
    ///     2: More UiElements we are tracking = More memory read
    ///     every X seconds just to update IsVisible :(.
    /// </summary>
    public class ImportantUiElements : RemoteObjectBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportantUiElements" /> class.
        /// </summary>
        /// <param name="address">
        ///     UiRoot 1st child address (starting from 0)
        ///     or <see cref="IntPtr.Zero" /> in case UiRoot has no child.
        /// </param>
        internal ImportantUiElements(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnTimeTick());
            CoroutineHandler.Start(this.OnMove(), priority: int.MaxValue - 1);
            CoroutineHandler.Start(this.OnForegroundChange(), priority: int.MaxValue - 1);
        }

        /// <summary>
        ///     Gets the LargeMap UiElement.
        ///     UiRoot -> MainChild -> 3rd index -> 1nd index.
        /// </summary>
        public LargeMapUiElement LargeMap { get; }

            = new(IntPtr.Zero);

        /// <summary>
        ///     Gets the MiniMap UiElement.
        ///     UiRoot -> MainChild -> 3rd index -> 2nd index.
        /// </summary>
        public MapUiElement MiniMap { get; }

            = new(IntPtr.Zero);

        /// <inheritdoc />
        protected override void CleanUpData()
        {
            this.MiniMap.Address = IntPtr.Zero;
            this.LargeMap.Address = IntPtr.Zero;
        }

        /// <inheritdoc />
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data1 = reader.ReadMemory<ImportantUiElementsOffsets>(this.Address);
            var data2 = reader.ReadMemory<MapParentStruct>(data1.MapParentPtr);

            // This won't throw an exception because (lucky us)
            // game UiElement garbage collection is not instant.
            // if this ever changes, put try catch on it.
            this.LargeMap.Address = data2.LargeMapPtr;
            this.MiniMap.Address = data2.MiniMapPtr;
        }

        private IEnumerator<Wait> OnTimeTick()
        {
            while (true)
            {
                yield return new Wait(0.1d);
                if (this.Address != IntPtr.Zero &&
                    Core.States.GameCurrentState == GameStateTypes.InGameState)
                {
                    // sending false because "true" use-case is handled
                    // by UpdateData function when address actually gets changed.
                    this.UpdateData(false);
                }
            }
        }

        private IEnumerator<Wait> OnMove()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnMoved);
                if (this.Address != IntPtr.Zero &&
                    Core.States.GameCurrentState == GameStateTypes.InGameState)
                {
                    // Reason for sending false given above.
                    this.UpdateData(false);
                }
            }
        }

        private IEnumerator<Wait> OnForegroundChange()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnForegroundChanged);
                if (this.Address != IntPtr.Zero &&
                    Core.States.GameCurrentState == GameStateTypes.InGameState)
                {
                    // Reason for sending false given above.
                    this.UpdateData(false);
                }
            }
        }
    }
}