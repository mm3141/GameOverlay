#nullable enable
namespace HealthBars.View
{
    using Entities;
    using GameHelper;
    using GameHelper.RemoteObjects.Components;
    using GameHelper.RemoteObjects.States.InGameStateObjects;

    /// <summary>
    /// </summary>
    public class EntityFactory
    {
        private readonly Enemy _enemy = new();
        private readonly Friendly _friendly = new();
        private readonly CurrentPlayer _player = new();

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEntity? GetEntity(Entity entity)
        {
            var hasVital = entity.TryGetComponent<Life>(out var entityLife);
            if (!hasVital || !entityLife.IsAlive)
            {
                return null;
            }

            var isBlockage = entity.TryGetComponent<TriggerableBlockage>(out _);
            if (isBlockage)
            {
                return null;
            }

            var hasRender = entity.TryGetComponent<Render>(out _);
            if (!hasRender)
            {
                return null;
            }

            var hasPositioned = entity.TryGetComponent<Positioned>(out var positioned);
            if (!hasPositioned)
            {
                return null;
            }

            var isPlayer = entity.TryGetComponent<Player>(out _);
            var hasMagicProperties = entity.TryGetComponent<ObjectMagicProperties>(out _);
            if (!hasMagicProperties && !isPlayer)
            {
                return null;
            }

            var willDieAfterTime = entity.TryGetComponent<DiesAfterTime>(out _);
            if (willDieAfterTime)
            {
                return null;
            }

            var isCurrentPlayer =
                entity.Address == Core.States.InGameStateObject.CurrentAreaInstance.Player.Address;

            if (isCurrentPlayer)
            {
                return this._player;
            }

            var isFriendly = positioned.IsFriendly;
            if (isFriendly)
            {
                return this._friendly;
            }

            return this._enemy;
        }
    }
}