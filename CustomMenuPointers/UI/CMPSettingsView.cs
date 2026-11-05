using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomMenuPointers.Configuration;
using CustomMenuPointers.Managers;
using Zenject;

namespace CustomMenuPointers.UI
{
    [ViewDefinition("CustomMenuPointers.UI.CMPSettingsView.bsml")]
    [HotReload(RelativePathToLayout = @"..\UI\CMPSettingsView.bsml")]
    public class CMPSettingsView : BSMLAutomaticViewController
    {
        private CMPManager _cmpManager;
        private PluginConfig _config;

        [Inject]
        internal void Construct(CMPManager cmpManager, PluginConfig config)
        {
            _cmpManager = cmpManager;
            _config = config;
        }

        [UIValue("cmp-enabled")]
        private bool CmpEnabled
        {
            get => _config.CmpEnabled;
            set => _ = _cmpManager.SetEnabled(value);
        }

        [UIAction("reload")]
        private void Reload() => _ = _cmpManager.Reload();

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            _cmpManager.RefreshColors();
        }
    }
}
