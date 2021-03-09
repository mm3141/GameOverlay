// <copyright file="GameWindowScale.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteObjects
{
    using System;
    using System.Collections.Generic;
    using Coroutine;
    using GameHelper.CoroutineEvents;
    using ImGuiNET;

    /// <summary>
    /// Reads the Game Window Scale values from the game.
    /// It's good to read from the game because there are 6 instances of them for different
    /// type of Ui-Elements. Only reads when game window moves/resize.
    /// </summary>
    public class GameWindowScale : RemoteObjectBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameWindowScale"/> class.
        /// </summary>
        /// <param name="address">address of the remote memory object.</param>
        internal GameWindowScale(IntPtr address)
            : base(address)
        {
            CoroutineHandler.Start(this.OnGameMove());
            CoroutineHandler.Start(this.OnGameForegroundChange());
        }

        /// <summary>
        /// Gets the current game window scale values.
        /// </summary>
        public float[] Values { get; private set; } = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };

        /// <summary>
        /// Gets the Scale Value depending on the index and multiplier.
        /// </summary>
        /// <param name="index">index read from the Ui-Element.</param>
        /// <param name="multiplier">multiplier read from the Ui-Element.</param>
        /// <returns>Returns the Width and Height scale value valid for the specific Ui-Element.</returns>
        public (float WidthScale, float HeightScale) GetScaleValue(int index, float multiplier)
        {
            float widthScale = multiplier;
            float heightScale = multiplier;
            switch (index)
            {
                case 1:
                    widthScale *= this.Values[2];
                    heightScale *= this.Values[3];
                    break;
                case 2:
                    widthScale *= this.Values[4];
                    heightScale *= this.Values[5];
                    break;
                case 3:
                    widthScale *= this.Values[0];
                    heightScale *= this.Values[1];
                    break;
                default:
                    break;
            }

            return (widthScale, heightScale);
        }

        /// <inheritdoc/>
        protected override void CleanUpData()
        {
            for (int i = 0; i < this.Values.Length; i++)
            {
                this.Values[i] = 1f;
            }
        }

        /// <inheritdoc/>
        protected override void UpdateData(bool hasAddressChanged)
        {
            var reader = Core.Process.Handle;
            var data = reader.ReadMemoryArray<float>(this.Address, this.Values.Length);
            for (int i = 0; i < data.Length; i++)
            {
                this.Values[i] = data[i];
            }
        }

        private IEnumerator<Wait> OnGameMove()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnMoved);

                // No need to check for IntPtr.zero
                // because game will only move when it exists. :D
                this.UpdateData(false);
            }
        }

        private IEnumerator<Wait> OnGameForegroundChange()
        {
            while (true)
            {
                yield return new Wait(GameHelperEvents.OnForegroundChanged);

                // No need to check for IntPtr.zero
                // because game will only move when it exists. :D
                this.UpdateData(false);
            }
        }
    }
}
