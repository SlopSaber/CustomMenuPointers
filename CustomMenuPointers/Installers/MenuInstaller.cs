using CustomMenuPointers.Managers;
using CustomMenuPointers.UI;
using CustomMenuPointers.UI.FlowCoordinators;
using Zenject;

namespace CustomMenuPointers.Installers
{
    internal class CMPMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<UIFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<CMPSettingsView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<CMPManager>().AsSingle();
            Container.BindInterfacesTo<MenuButtonUI>().AsSingle();
        }
    }
}
