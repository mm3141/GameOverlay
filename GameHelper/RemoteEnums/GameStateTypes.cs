// <copyright file="GameStateTypes.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteEnums
{
    /// <summary>
    ///     Gets all known states of the game.
    /// </summary>
    public enum GameStateTypes
    {
        /// <summary>
        ///     When user is in Area Loading Screen.
        /// </summary>
        AreaLoadingState,

        /// <summary>
        ///     Game state.
        /// </summary>
        WaitingState,

        /// <summary>
        ///     When user is in Login Screen.
        /// </summary>
        CreditsState,

        /// <summary>
        ///     Game State.
        /// </summary>
        EscapeState,

        /// <summary>
        ///     When User is in Town/Hideout/Area/Zone etc.
        /// </summary>
        InGameState,

        /// <summary>
        ///     Game State.
        /// </summary>
        ChangePasswordState,

        /// <summary>
        ///     Game State.
        /// </summary>
        LoginState,

        /// <summary>
        ///     Game State.
        /// </summary>
        PreGameState,

        /// <summary>
        ///     Game State.
        /// </summary>
        CreateCharacterState,

        /// <summary>
        ///     Game State.
        /// </summary>
        SelectCharacterState,

        /// <summary>
        ///     Game State.
        /// </summary>
        DeleteCharacterState,

        /// <summary>
        ///     Game State.
        /// </summary>
        LoadingState,

        /// <summary>
        ///     This is a special State, it will not trigger State Change Event.
        ///     This is just for displaying purposes. It means Game isn't stared.
        /// </summary>
        GameNotLoaded
    }
}