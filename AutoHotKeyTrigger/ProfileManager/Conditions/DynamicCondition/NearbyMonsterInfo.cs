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
                    EntityTypes.FriendlyMonster or
                    EntityTypes.Monster or
                    EntityTypes.Stage1FIT or
                    EntityTypes.Stage1EChestFIT or
                    EntityTypes.Stage1RewardFIT))
                {
                    continue;
                }

                if (entity.EntityType == EntityTypes.FriendlyMonster)
                {
                    this.FriendlyMonsterCount++;
                }
                else
                {
                    entity.TryGetComponent<ObjectMagicProperties>(out var omp);

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
