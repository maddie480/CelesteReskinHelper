namespace Celeste.Mod.ReskinHelper {
    /// <summary>
    /// This is the structure of the yaml files used to set up a custom skin.
    /// To be picked up by Reskin Helper, they should be placed in a ReskinHelperSkins folder.
    /// </summary>
    public class ReskinSettingsFile {
        public class PlayerSettings {
            // whether the player should have hair or not
            public bool HasHair { get; set; } = true;

            // hair colors (also influences the colors of the orbs when Madeline dies and respawns)
            public string ZeroDashHairColor { get; set; } = null;
            public string OneDashHairColor { get; set; } = null;
            public string TwoDashHairColor { get; set; } = null;

            // color grades to apply to Madeline depending on dash count (useful for color swaps)
            public string ZeroDashColorGrade { get; set; } = null;
            public string OneDashColorGrade { get; set; } = null;
            public string TwoDashColorGrade { get; set; } = null;

            // dash particle colors (particles blink between colors 1 and 2)
            public string ZeroDashParticleColor1 { get; set; } = null;
            public string ZeroDashParticleColor2 { get; set; } = null;
            public string OneDashParticleColor1 { get; set; } = null;
            public string OneDashParticleColor2 { get; set; } = null;

            // name of the animation to use for the player in Graphics/Sprites.xml
            public string XmlSpriteName { get; set; } = null;
        }

        // settings for the player sprite
        public PlayerSettings Player { get; set; } = null;

        // the dialog ID of the skin name in Dialog/English.txt. Used to display the skin name in the selector in Mod Options
        // if not specified, it will default to reskinhelper_skinname_{pathtotheyaml}
        public string SkinNameInEnglishTxt { get; set; } = null;
    }
}
