using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Mod.ReskinHelper {
    public class ReskinHelperModule : EverestModule {
        // contains all skins that exist, the key being the path to the settings yaml file (used as an ID).
        public static Dictionary<string, ReskinSettingsFile> ReskinSettingsFiles = new Dictionary<string, ReskinSettingsFile>();

        public static ReskinHelperModule Instance;

        public override Type SettingsType => typeof(ReskinHelperSettings);
        public static ReskinHelperSettings Settings => (ReskinHelperSettings) Instance._Settings;

        public ReskinHelperModule() {
            Instance = this;
        }

        public override void Load() {
            PlayerReskinHelper.Load();
            Everest.Content.OnUpdate += onModAssetUpdate;
            On.Celeste.LevelLoader.LoadingThread += onLevelLoadingThread;
        }

        public override void Unload() {
            PlayerReskinHelper.Unload();
            Everest.Content.OnUpdate -= onModAssetUpdate;
            On.Celeste.LevelLoader.LoadingThread -= onLevelLoadingThread;
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);

            // initial load of the settings files
            reloadReskinSettingsFiles();
        }

        private void onModAssetUpdate(ModAsset oldAsset, ModAsset newAsset) {
            if (newAsset != null && newAsset.PathVirtual.StartsWith("ReskinHelperSkins/")) {
                // reload settings files when one is added or updated.
                reloadReskinSettingsFiles();
            }
        }

        private void onLevelLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            orig(self);

            // we want to load sprites metadata (hair position) for all custom player sprites.
            foreach (ReskinSettingsFile file in ReskinSettingsFiles.Values) {
                string xmlName = file.Player?.XmlSpriteName;
                if (xmlName != null) {
                    PlayerSprite.CreateFramesMetadata(xmlName);
                }
            }
        }

        private static void reloadReskinSettingsFiles() {
            ReskinSettingsFiles.Clear();

            Logger.Log("ReskinHelper", "Scanning for skin definitions...");

            // scan for yaml files contained in a ReskinHelperSkins folder.
            foreach (string yamlPath in Everest.Content.Map
                .Where(path => path.Value.Type == typeof(AssetTypeYaml) && path.Key.StartsWith("ReskinHelperSkins/"))
                .Select(path => path.Key)) {

                Logger.Log("ReskinHelper", $"Loading skin definition: {yamlPath}");
                ReskinSettingsFile settings = YamlHelper.Deserializer.Deserialize<ReskinSettingsFile>(new StreamReader(Everest.Content.Map[yamlPath].Stream));
                ReskinSettingsFiles.Add(yamlPath, settings);
            }

            // load settings again: we want to (re-)apply settings now that they're loaded.
            Instance.LoadSettings();
        }
    }
}
