using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.ReskinHelper {
    public class ReskinHelperSettings : EverestModuleSettings {
        private string reskinName = null;

        public string ReskinName {
            get => reskinName;
            set {
                reskinName = value;

                ReskinSettingsFile settings = null;
                if (reskinName != null) {
                    ReskinHelperModule.ReskinSettingsFiles.TryGetValue(reskinName, out settings);
                }

                string xmlSpriteNameBeforeSwitch = PlayerReskinHelper.XmlSpriteName;

                if (settings == null || settings.Player == null) {
                    // there is no skin, currently selected skin doesn't exist anymore, or it has no player customization: reset everything!
                    PlayerReskinHelper.ZeroDashColor = null;
                    PlayerReskinHelper.OneDashColor = null;
                    PlayerReskinHelper.TwoDashColor = null;

                    PlayerReskinHelper.ZeroDashColorGrade = null;
                    PlayerReskinHelper.OneDashColorGrade = null;
                    PlayerReskinHelper.TwoDashColorGrade = null;

                    PlayerReskinHelper.P_DashA = null;
                    PlayerReskinHelper.P_DashB = null;

                    PlayerReskinHelper.XmlSpriteName = null;

                    PlayerReskinHelper.HasHair = true;
                } else {
                    // set up the new player settings, parsing colors and instanciating the particles.
                    PlayerReskinHelper.ZeroDashColor = settings.Player.ZeroDashHairColor == null ? (Color?) null : Calc.HexToColor(settings.Player.ZeroDashHairColor);
                    PlayerReskinHelper.OneDashColor = settings.Player.OneDashHairColor == null ? (Color?) null : Calc.HexToColor(settings.Player.OneDashHairColor);
                    PlayerReskinHelper.TwoDashColor = settings.Player.TwoDashHairColor == null ? (Color?) null : Calc.HexToColor(settings.Player.TwoDashHairColor);

                    PlayerReskinHelper.ZeroDashColorGrade = settings.Player.ZeroDashColorGrade;
                    PlayerReskinHelper.OneDashColorGrade = settings.Player.OneDashColorGrade;
                    PlayerReskinHelper.TwoDashColorGrade = settings.Player.TwoDashColorGrade;

                    PlayerReskinHelper.P_DashA = settings.Player.ZeroDashParticleColor1 == null ? null : new ParticleType(Player.P_DashA) {
                        Color = Calc.HexToColor(settings.Player.ZeroDashParticleColor1),
                        Color2 = Calc.HexToColor(settings.Player.ZeroDashParticleColor2)
                    };
                    PlayerReskinHelper.P_DashB = settings.Player.OneDashParticleColor1 == null ? null : new ParticleType(Player.P_DashB) {
                        Color = Calc.HexToColor(settings.Player.OneDashParticleColor1),
                        Color2 = Calc.HexToColor(settings.Player.OneDashParticleColor2)
                    };

                    PlayerReskinHelper.XmlSpriteName = settings.Player.XmlSpriteName;

                    PlayerReskinHelper.HasHair = settings.Player.HasHair;
                }

                if (Engine.Scene is Level level && PlayerReskinHelper.XmlSpriteName != xmlSpriteNameBeforeSwitch) {
                    // we're in a map: reset sprite the same way the Other Self toggle does.
                    // the hook on PlayerSprite will apply the skin (or not).
                    Player player = level.Tracker.GetEntity<Player>();
                    if (player != null) {
                        PlayerSpriteMode mode = (SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode);
                        if (player.Active) {
                            player.ResetSpriteNextFrame(mode);
                        } else {
                            player.ResetSprite(mode);
                        }
                    }
                }
            }
        }

        public void CreateReskinNameEntry(TextMenu menu, bool inGame) {
            int selectedIndex = 0;
            List<string> optionNames = new List<string>();
            List<string> optionValues = new List<string>();

            // this is the "skins are disabled" option.
            optionNames.Add(Dialog.Clean("reskinhelper_noskin"));
            optionValues.Add(null);

            foreach (string skin in ReskinHelperModule.ReskinSettingsFiles.Keys) {
                if (ReskinName == skin) {
                    // this is the currently selected skin, so we want to select it by default.
                    selectedIndex = optionNames.Count;
                }

                // the name is either the key defined in the yaml, or reskinhelper_skinname_{pathtotheyaml} if it isn't defined.
                optionNames.Add(Dialog.Clean(ReskinHelperModule.ReskinSettingsFiles[skin].SkinNameInEnglishTxt ?? ("reskinhelper_skinname_" + skin.DialogKeyify())));
                optionValues.Add(skin);
            }

            menu.Add(new TextMenu.Slider(Dialog.Clean("reskinhelper_skinoption"), i => optionNames[i], 0, optionNames.Count - 1, selectedIndex)
                .Change(i => ReskinName = optionValues[i]));
        }
    }
}
