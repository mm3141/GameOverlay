// <copyright file="DynamicConditionState.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AutoHotKeyTrigger.ProfileManager.Conditions.DynamicCondition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.RemoteObjects.States;
    using Interface;
    using ClickableTransparentOverlay;

    /// <summary>
    ///     The structure that can be queried using DynamicCondition
    /// </summary>
    public class DynamicConditionState : IDynamicConditionState
    {
        private readonly Lazy<NearbyMonsterInfo> nearbyMonsterInfo;

        /// <summary>
        ///     Creates a new instance
        /// </summary>
        /// <param name="state">State to build the structure from</param>
        public DynamicConditionState(InGameState state)
        {
            if (state != null)
            {
                var player = state.CurrentAreaInstance.Player;
                if (player.GetComp<Buffs>(out var playerBuffs))
                {
                    this.Ailments = JsonDataHelper.StatusEffectGroups
                                                  .Where(x => x.Value.Any(playerBuffs.StatusEffects.ContainsKey))
                                                  .Select(x => x.Key).ToHashSet();
                    this.Buffs = new BuffDictionary(playerBuffs.StatusEffects);
                }

                if (player.GetComp<Actor>(out var actorComponent))
                {
                    this.Animation = actorComponent.Animation;
                }

                if (player.GetComp<Life>(out var lifeComponent))
                {
                    this.Vitals = new VitalsInfo(lifeComponent);
                }

                this.Flasks = new FlasksInfo(state);
                this.nearbyMonsterInfo = new Lazy<NearbyMonsterInfo>(() => new NearbyMonsterInfo(state));
            }
        }

        /// <summary>
        ///     The buff list
        /// </summary>
        public IBuffDictionary Buffs { get; }

        /// <summary>
        ///     The current animation
        /// </summary>
        public Animation Animation { get; }

        /// <summary>
        ///     The ailment list
        /// </summary>
        public IReadOnlyCollection<string> Ailments { get; } = new List<string>();

        /// <summary>
        ///     The vitals information
        /// </summary>
        public IVitalsInfo Vitals { get; }

        /// <summary>
        ///     The flask information
        /// </summary>
        public IFlasksInfo Flasks { get; }

        /// <summary>
        ///     Calculates the number of nearby monsters given a rarity selector
        /// </summary>
        /// <param name="rarity">The rarity selector for monster search</param>
        /// <returns></returns>
        public int MonsterCount(MonsterRarity rarity) => this.nearbyMonsterInfo.Value.GetMonsterCount(rarity);

        /// <summary>
        ///     Number of friendly nearby monsters
        /// </summary>
        public int FriendlyMonsterCount => this.nearbyMonsterInfo.Value.FriendlyMonsterCount;
        
        /// <summary>
        ///     Capture the key press event
        /// </summary>
        public bool IsKeyPressedForAction(int vk) => NativeMethods.IsKeyPressed(vk);        
    }
}
