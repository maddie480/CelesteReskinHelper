using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.ReskinHelper {
    /// <summary>
    /// This file controls the special features related to the player sprite (hair colors, color grades, etc).
    /// </summary>
    public static class PlayerReskinHelper {
        // the dash colors
        public static Color? ZeroDashColor = null;
        public static Color? OneDashColor = null;
        public static Color? TwoDashColor = null;

        // the dash color grades
        public static string ZeroDashColorGrade = null;
        public static string OneDashColorGrade = null;
        public static string TwoDashColorGrade = null;

        // recolored dash particles (A = 0-dash, B = 1-dash)
        public static ParticleType P_DashA = null;
        public static ParticleType P_DashB = null;

        // sprite name in Sprites.xml
        public static string XmlSpriteName = null;

        // toggle for the hair
        public static bool HasHair = true;

        public static void Load() {
            On.Celeste.PlayerSprite.ctor += onPlayerSpriteConstructor;
            On.Celeste.Player.Render += onPlayerRender;
            On.Celeste.Player.GetCurrentTrailColor += onPlayerGetTrailColor;
            On.Celeste.Player.UpdateHair += onPlayerUpdateHair;
            On.Celeste.Player.DashUpdate += onPlayerDashUpdate;
            On.Celeste.PlayerHair.Render += onPlayerHairRender;
        }

        public static void Unload() {
            On.Celeste.PlayerSprite.ctor -= onPlayerSpriteConstructor;
            On.Celeste.Player.Render -= onPlayerRender;
            On.Celeste.Player.GetCurrentTrailColor -= onPlayerGetTrailColor;
            On.Celeste.Player.UpdateHair -= onPlayerUpdateHair;
            On.Celeste.Player.DashUpdate -= onPlayerDashUpdate;
            On.Celeste.PlayerHair.Render -= onPlayerHairRender;
        }

        private static void onPlayerSpriteConstructor(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode) {
            orig(self, mode);

            if (XmlSpriteName != null && (mode == PlayerSpriteMode.Madeline || mode == PlayerSpriteMode.MadelineAsBadeline || mode == PlayerSpriteMode.MadelineNoBackpack)) {
                // replace the sprite with the defined sprite (from Graphics/Sprites.xml).
                GFX.SpriteBank.CreateOn(self, XmlSpriteName);
            }
        }

        private static void onPlayerRender(On.Celeste.Player.orig_Render orig, Player self) {
            // we are going to apply a color grade to Maddy to indicate dash count.
            string id;
            if (self.Dashes == 0) {
                id = ZeroDashColorGrade;
            } else if (self.Dashes == 1) {
                id = OneDashColorGrade;
            } else {
                id = TwoDashColorGrade;
            }

            if (id == null) {
                // no replacement to do, original sprites are the right color.
                orig(self);
                return;
            }

            // initialize color grade...
            Effect fxColorGrading = GFX.FxColorGrading;
            fxColorGrading.CurrentTechnique = fxColorGrading.Techniques["ColorGradeSingle"];
            Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[id].Texture.Texture_Safe;
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, fxColorGrading, (self.Scene as Level).GameplayRenderer.Camera.Matrix);

            // render Maddy...
            orig(self);

            // ... and reset rendering to stop using the color grade.
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
        }

        private static Color onPlayerGetTrailColor(On.Celeste.Player.orig_GetCurrentTrailColor orig, Player self) {
            // replace trail colors with hair colors
            int dashCount = self.Dashes;
            if (dashCount == 0) {
                return ZeroDashColor ?? orig(self);
            } else if (dashCount == 1) {
                return OneDashColor ?? orig(self);
            } else {
                return TwoDashColor ?? orig(self);
            }
        }

        private static void onPlayerUpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity) {
            orig(self, applyGravity);

            // change player hair color to match dash colors.
            // (when hair is invisible, that influences other things like the orbs when Maddy dies and respawns)
            int dashCount = self.Dashes;
            Color? targetColor = null;
            if (dashCount == 0) {
                targetColor = ZeroDashColor;
            } else if (dashCount == 1) {
                targetColor = OneDashColor;
            } else {
                targetColor = TwoDashColor;
            }

            if (targetColor.HasValue) {
                self.Hair.Color = targetColor.Value;
            }
        }

        private static int onPlayerDashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self) {
            if (P_DashA == null && P_DashB == null) {
                // skin disabled: just run vanilla code
                return orig(self);
            }

            // back up vanilla particles
            ParticleType bakDashA = Player.P_DashA;
            ParticleType bakDashB = Player.P_DashB;
            ParticleType bakDashBadB = Player.P_DashBadB;

            // replace them with our recolored ones
            Player.P_DashA = P_DashA;
            Player.P_DashB = P_DashB;
            Player.P_DashBadB = P_DashB;

            // run vanilla code: if it emits particles, it will use our recolored ones.
            int result = orig(self);

            // restore vanilla particles
            Player.P_DashA = bakDashA;
            Player.P_DashB = bakDashB;
            Player.P_DashBadB = bakDashBadB;

            return result;
        }

        private static void onPlayerHairRender(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self) {
            // display hair only if it is enabled, or that it doesn't belong to a Player entity.
            if (HasHair || !(self.Entity is Player)) {
                orig(self);
            }
        }
    }
}
