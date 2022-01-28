namespace SimpleFlaskManager.ProfileManager.Conditions.DynamicCondition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameHelper.RemoteObjects.States;
    using Interface;

    /// <summary>
    ///     Information about a set of flasks
    /// </summary>
    public class FlasksInfo : IFlasksInfo
    {
        private const int FlaskCount = 5;

        /// <summary>
        ///     Provides access to the flask array
        /// </summary>
        /// <param name="i">The flask index (0-based)</param>
        public IFlaskInfo this[int i]
        {
            get
            {
                if (i < 0 || i >= FlaskCount)
                {
                    throw new Exception($"Flask index is 0-based and must be in the range of 0-{FlaskCount - 1}");
                }

                return this.flasks[i];
            }
        }

        private readonly IReadOnlyList<FlaskInfo> flasks;

        /// <summary>
        ///     Creates a new instance
        /// </summary>
        /// <param name="state">State to build the structure from</param>
        public FlasksInfo(InGameState state)
        {
            this.flasks = Enumerable.Range(0, FlaskCount)
                                    .Select(i => state.CurrentAreaInstance.ServerDataObject.FlaskInventory[0, i])
                                    .Select(f => FlaskInfo.From(state, f))
                                    .ToList();
        }
    }
}
