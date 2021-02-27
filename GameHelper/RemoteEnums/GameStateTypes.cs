// <copyright file="GameStateTypes.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteEnums
{
    /// <summary>
    /// Gets all known states of the game.
    /// NOTE: GameNotLoaded means game isn't up and running.
    /// </summary>
    public enum GameStateTypes
    {
#pragma warning disable CS1591, SA1602
        GameNotLoaded,
        InGameState,
        PreGameState,
        WaitingState,
        SelectCharacterState,
        LoadingState,
        LoginState,
        CreateCharacterState,
        DeleteCharacterState,
        EscapeState,
        CreditsState,
        AreaLoadingState,
        ChangePasswordState,
#pragma warning restore CS1591, SA1602
    }
}