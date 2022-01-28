namespace SimpleFlaskManager.ProfileManager.Conditions.DynamicCondition.Interface
{
    /// <summary>
    ///     The structure describing a flask state
    /// </summary>
    public interface IFlaskInfo
    {
        /// <summary>
        ///     Whether the flask effect is active now
        /// </summary>
        bool Active { get; init; }

        /// <summary>
        ///     Current charge amount of a flask
        /// </summary>
        int Charges { get; init; }
    }
}
