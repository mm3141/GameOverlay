// <copyright file="ImportantUiElements.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects.States.InGameStateObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.RemoteObjects.UiElement;
    using GameOffsets.Objects.States.InGameState;

    /// <summary>
    /// Points to all the important Ui Elements in the game
    /// and keeps them up to date.
    /// </summary>
    public class ImportantUiElements : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportantUiElements"/> class.
        /// </summary>
        /// <param name="address">address of the structure containing all the Ui Objects.</param>
        internal ImportantUiElements(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnTimeTick());
        }

        /// <summary>
        /// Gets the Mini/Large Map Ui element.
        /// </summary>
        public UiElementBase Map
        {
            get;
            private set;
        }

        = new UiElementBase(IntPtr.Zero);

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemory<ImportantUiElementsOffsets>(this.Address);
            this.Map.Address = data.Map;
        }

        private IEnumerator<Wait> OnTimeTick()
        {
            while (true)
            {
                yield return new Wait(0.5d);
                if (this.Address != IntPtr.Zero)
                {
                    this.UpdateData(false);
                }
            }
        }
    }
}
