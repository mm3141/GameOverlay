namespace AutoHotKeyTrigger.ProfileManager.Conditions.DynamicCondition
{
    using GameHelper.RemoteEnums;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.RemoteObjects.States;
    using Interface;

    /// <summary>
    ///     Stores optimized information about nearby monster count
    /// </summary>
    public class NearbyMonsterInfo
    {
        private readonly int nearbyNormalMonsterCount;
        private readonly int nearbyMagicMonsterCount;
        private readonly int nearbyRareMonsterCount;
        private readonly int nearbyUniqueMonsterCount;

        /// <summary>
        ///     Creates a new instance of <see cref="NearbyMonsterInfo"/>
        /// </summary>
        /// <param name="state"></param>
        public NearbyMonsterInfo(InGameState state)
        {
            foreach (var entity in state.CurrentAreaInstance.AwakeEntities.Values)
            {
                if (!entity.IsNearby)
                {
                    continue;
                }

                if (entity.EntityType is not (
                    eTypes.FriendlyMonster or
                    eTypes.Monster or
                    eTypes.Stage1FIT or
                    eTypes.Stage1EChestFIT or
                    eTypes.Stage1RewardFIT))
                {
                    continue;
                }

                if (entity.EntityType == eTypes.FriendlyMonster)
                {
                    this.FriendlyMonsterCount++;
                }
                else
                {
                    entity.GetComp<ObjectMagicProperties>(out var omp);

                    switch (omp.Rarity)
                    {
                        case Rarity.Normal:
                            this.nearbyNormalMonsterCount++;
                            break;
                        case Rarity.Magic:
                            this.nearbyMagicMonsterCount++;
                            break;
                        case Rarity.Rare:
                            this.nearbyRareMonsterCount++;
                            break;
                        case Rarity.Unique:
                            this.nearbyUniqueMonsterCount++;
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Number of friendly nearby monsters
        /// </summary>
        public int FriendlyMonsterCount { get; }

        /// <summary>
        /// Calculates the nearby monster count, see <see cref="DynamicConditionState.MonsterCount"/> for details
        /// </summary>
        public int GetMonsterCount(MonsterRarity rarity)
        {
            var sum = 0;
            if (rarity.HasFlag(MonsterRarity.Normal))
            {
                sum += this.nearbyNormalMonsterCount;
            }

            if (rarity.HasFlag(MonsterRarity.Magic))
            {
                sum += this.nearbyMagicMonsterCount;
            }

            if (rarity.HasFlag(MonsterRarity.Rare))
            {
                sum += this.nearbyRareMonsterCount;
            }

            if (rarity.HasFlag(MonsterRarity.Unique))
            {
                sum += this.nearbyUniqueMonsterCount;
            }

            return sum;
        }
    }
}
