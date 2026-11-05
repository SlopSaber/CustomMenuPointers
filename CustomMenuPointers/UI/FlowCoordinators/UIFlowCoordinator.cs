using System;
using BeatSaberMarkupLanguage;
using CustomMenuPointers.UI;
using HMUI;
using SiraUtil.Logging;
using SiraUtil.Tools;
using Zenject;

namespace CustomMenuPointers.UI.FlowCoordinators
{
    public class UIFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        private CMPSettingsView _cmpSettingsView;
        private SiraLog _siraLog;

        [Inject]
        public void Construct(MainFlowCoordinator mainFlowCoordinator, CMPSettingsView cmpSettingsView, SiraLog siraLog)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _cmpSettingsView = cmpSettingsView;
            _siraLog = siraLog;
        }
        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (firstActivation)
                {
                    SetTitle("Menu Pointers");
                    showBackButton = true;
                    ProvideInitialViewControllers(_cmpSettingsView);
                }
            }
            catch (Exception ex)
            {
                _siraLog.Error(ex);
            }
            
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}