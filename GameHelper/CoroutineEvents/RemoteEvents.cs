// <copyright file="RemoteEvents.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.CoroutineEvents
{
    using Coroutine;

    /// <summary>
    /// Co-Routine events specific to the game.
    /// </summary>
    public static class RemoteEvents
    {
        /// <summary>
        /// Gets the event that is triggered 20ms after the area change detection.
        /// Area change is detected by checking if the time spend on the
        /// loading screen (given to us by the game) is greater than the
        /// last recorded time.
        /// </summary>
        public static readonly Event AreaChanged = new Event();

        /// <summary>
        /// Gets the Area Change Detection event. When Area Change happens many
        /// classes want to gather new data from the new Area. Unfortunately, Some of
        /// the those classes have inter-dependicies with each other.
        /// This event is called before AreaChanged event, so this event can
        /// be used to resolve some inter-dependicies. This is required since co-routine
        /// doesn't guarantees a certian order in execution of functions. In the future,
        /// if inter-dependicies gets out of control we might have to ditch co-routines
        /// for AreaChange scenario. So, use this event if your class can independently
        /// UpdateData, otherwise use AreaChanged. This class data is already loaded when
        /// this event is called.
        /// </summary>
        internal static readonly Event AreaChangeDetected = new Event();

        /// <summary>
        /// Gets the Current State Changed event.
        /// </summary>
        internal static readonly Event StateChanged = new Event();
    }
}
