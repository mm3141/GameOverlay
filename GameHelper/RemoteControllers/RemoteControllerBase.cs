// <copyright file="RemoteControllerBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteControllers
{
    using System;
    using Coroutine;

    /// <summary>
    /// An abstract class to create the <see cref="RemoteControllers"/>.
    /// RemoteControllers are basically objects with static
    /// data in them i.e. their data never changes
    /// while the game is up and running. They are only
    /// updated when the game restarts. If you are thinking
    /// of creating a Controller whos data refreshes again and
    /// again while the game is running, make it a <see cref="RemoteObjects"/>.
    /// </summary>
    public abstract class RemoteControllerBase
    {
        private IntPtr address = IntPtr.Zero;

        /// <summary>
        /// Gets or sets this controller address.
        /// Setting the value to IntPtr.Zero will disable the controller and
        /// all the subsequent classes coroutine from executing.
        /// </summary>
        public IntPtr Address
        {
            get => this.address;
            set
            {
                this.address = value;
                this.OnAddressUpdated(value);
            }
        }

        /// <summary>
        /// Gets the event indicating if the Controller is Ready or not.
        /// Ideally, this should only be triggered once per game.
        /// </summary>
        internal Event OnControllerReady { get; private set; } = new Event();

        /// <summary>
        /// Work to do when the Address is updated.
        /// </summary>
        /// <param name="newAddress">New address value.</param>
        protected abstract void OnAddressUpdated(IntPtr newAddress);
    }
}
