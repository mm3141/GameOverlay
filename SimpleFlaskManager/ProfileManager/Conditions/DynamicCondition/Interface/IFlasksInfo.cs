namespace SimpleFlaskManager.ProfileManager.Conditions.DynamicCondition.Interface
{
    /// <summary>
    ///     Information about a set of flasks
    /// </summary>
    public interface IFlasksInfo
    {
        /// <summary>
        ///     Provides access to the flask array
        /// </summary>
        /// <param name="i">The flask index (0-based)</param>
        IFlaskInfo this[int i] { get; }
    }
}
