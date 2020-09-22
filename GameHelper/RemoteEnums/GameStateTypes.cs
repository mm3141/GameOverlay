// <copyright file="GameStateTypes.cs" company="None">
// Copyright (c) None. All rights reserved.
// </copyright>

namespace GameHelper.RemoteEnums
{
    /// <summary>
    /// Gets all known states of the game.
    /// NOTE: GameNotLoaded means game isn't up and running.
    /// NOTE: In case of new enum, do update <see cref="CurrentState"/> class.
    /// </summary>
    public enum GameStateTypes
    {
#pragma warning disable SA1602 // Enumeration items should be documented
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
#pragma warning restore SA1602 // Enumeration items should be documented
    }
}