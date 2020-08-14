// <copyright file="ControllerBase.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.Controllers
{
    using System;
    using Coroutine;

    /// <summary>
    /// An abstract class to create the controllers.
    /// Controllers are basically objects with static
    /// data in them i.e. their data never changes
    /// while the game is up and running. They are only
    /// updated when the game restarts. If you are thinking
    /// of creating a Controller whos data refreshes again and
    /// again while the game is running, make it a RemoteMemoryObject
    /// rather than a controller.
    ///
    /// NOTE: all controllers uses (time/event based) coroutines
    /// to process data or communicate/inform other controllers/classes
    /// when something has happened.
    /// </summary>
    internal abstract class ControllerBase
    {
        private IntPtr address = IntPtr.Zero;

        /// <summary>
        /// Gets or sets this controller address.
        /// Setting the value to IntPtr.Zero will disable the controller and
        /// all the subsequent classes coroutine from executing.
        /// </summary>
        internal IntPtr Address
        {
            get => this.address;
            set
            {
                this.address = value;
                this.OnAddressUpdated(value);
            }
        }

        /// <summary>
        /// Gets the Controller Ready to use event. The purpose
        /// of this event is to let the other classes/controllers
        /// know when this controller has properly initilized and is
        /// ready to use.
        ///
        /// NOTE: Normally this is triggered once per game restart
        /// since controllers addresses/data never changes during the game.
        /// </summary>
        internal Event OnControllerReady { get; private set; } = new Event();

        /// <summary>
        /// Work to do when the Address is updated.
        /// </summary>
        /// <param name="newAddress">New address value.</param>
        protected abstract void OnAddressUpdated(IntPtr newAddress);
    }
}
