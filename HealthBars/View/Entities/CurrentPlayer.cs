namespace HealthBars.View.Entities {
    /// <inheritdoc />
    public class CurrentPlayer : CommonFriendly {
        /// <inheritdoc />
        public override void Draw(EntityParams eP, SpriteController spriteController) {
            var scale = RarityBarScale(eP);

            AddDoubleEmptyBar(spriteController, eP, scale);
            AddManaBar(spriteController, eP, scale, "Mana");
            AddHealthBar(spriteController, eP, scale);
            if (eP.EsTotal > 0) {
                AddEnergyShieldBar(spriteController, eP, scale);
            }
        }

        /// <inheritdoc />
        public override bool ShouldDraw(EntityParams entityParams) {
            return entityParams.Settings.ShowPlayerBars;
        }

        private static float RarityBarScale(EntityParams entityParams) {
            return entityParams.Settings.PlayerBarScale;
        }
    }
}