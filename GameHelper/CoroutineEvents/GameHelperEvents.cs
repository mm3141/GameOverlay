// <copyright file="GameHelperEvents.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.CoroutineEvents
{
    using Coroutine;

    /// <summary>
    /// Co-Routine events specific to the GameHelper.
    /// </summary>
    public static class GameHelperEvents
    {
        /// <summary>
        /// To Update data every frame before rendering.
        /// </summary>
        internal static readonly Event PerFrameDataUpdate = new Event();

        /// <summary>
        /// To submit ImGui code for generating the UI.
        /// </summary>
        internal static readonly Event OnRender = new Event();

        /// <summary>
        /// Gets the event raised when GameProcess has opened a new game.
        /// </summary>
        internal static readonly Event OnOpened = new Event();

        /// <summary>
        /// Gets the event raised just before the GameProcess has closed the game.
        /// </summary>
        internal static readonly Event OnClose = new Event();

        /// <summary>
        /// Gets the event raised when the game has changed its size, position or both.
        /// </summary>
        internal static readonly Event OnMoved = new Event();

        /// <summary>
        /// Gets the event raised when the game Foreground property has changed.
        /// </summary>
        internal static readonly Event OnForegroundChanged = new Event();

        /// <summary>
        /// Gets the Game Helper closing event. The event is called whenever
        /// all the settings have to be saved.
        /// </summary>
        internal static readonly Event TimeToSaveAllSettings = new Event();
    }
}
