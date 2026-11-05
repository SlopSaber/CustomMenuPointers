using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomMenuPointers.Configuration;
using CustomSabersLite;
using SabersCore.Models;
using SabersCore.Services;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace CustomMenuPointers.Managers
{
    internal class CMPManager : IInitializable, IDisposable
    {
        private static readonly string[] HandleRendererNames = { "Glowing", "Normal", "FakeGlow0", "FakeGlow1" };

        private readonly ISaberInstanceFactory _saberInstanceFactory;
        private readonly PlayerDataModel _playerDataModel;
        private readonly MenuPlayerController _menuPlayerController;
        private readonly PluginConfig _config;
        private readonly SiraLog _log;
        private readonly List<Renderer> _handleRenderers = new List<Renderer>();

        private Transform _leftHandle;
        private Transform _rightHandle;
        private SaberInstanceSet _sabers;
        private CancellationTokenSource _loading;

        public CMPManager(
            ISaberInstanceFactory saberInstanceFactory,
            PlayerDataModel playerDataModel,
            MenuPlayerController menuPlayerController,
            PluginConfig config,
            SiraLog log)
        {
            _saberInstanceFactory = saberInstanceFactory;
            _playerDataModel = playerDataModel;
            _menuPlayerController = menuPlayerController;
            _config = config;
            _log = log;
        }

        public void Initialize()
        {
            _leftHandle = FindHandle(_menuPlayerController.leftController);
            _rightHandle = FindHandle(_menuPlayerController.rightController);

            SaberSelection.SelectedSaberChanged += OnSelectedSaberChanged;
            SaberSelection.HeldPreviewChanged += OnHeldPreviewChanged;
            _ = Reload();
        }

        public void Dispose()
        {
            SaberSelection.SelectedSaberChanged -= OnSelectedSaberChanged;
            SaberSelection.HeldPreviewChanged -= OnHeldPreviewChanged;
            CancelLoading();
            DestroySabers();
        }

        public Task SetEnabled(bool enabled)
        {
            _config.CmpEnabled = enabled;
            return Reload();
        }

        public async Task Reload()
        {
            CancelLoading();
            DestroySabers();

            var hash = SaberSelection.SelectedSaberHash;
            if (!_config.CmpEnabled || hash == null || _leftHandle == null || _rightHandle == null)
            {
                UpdateVisibility();
                return;
            }

            var loading = _loading = new CancellationTokenSource();
            SaberInstanceSet sabers;
            try
            {
                sabers = await _saberInstanceFactory.CreateSaberSet(hash, loading.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _log.Error($"Could not create menu pointers for saber {hash}");
                _log.Error(ex);
                UpdateVisibility();
                return;
            }

            if (loading.IsCancellationRequested || _loading != loading)
            {
                sabers.Dispose();
                return;
            }

            _sabers = sabers;
            Attach(sabers.LeftSaber, _leftHandle);
            Attach(sabers.RightSaber, _rightHandle);
            RefreshColors();
            UpdateVisibility();
        }

        public void RefreshColors()
        {
            if (_sabers == null)
            {
                return;
            }

            var colorScheme = _playerDataModel.playerData.colorSchemesSettings.GetSelectedColorScheme();
            _sabers.LeftSaber?.SetColorScheme(colorScheme);
            _sabers.RightSaber?.SetColorScheme(colorScheme);
        }

        private static Transform FindHandle(VRController controller) =>
            controller == null ? null : controller.transform.Find("MenuHandle");

        private static void Attach(ISaber saber, Transform handle)
        {
            if (saber == null)
            {
                return;
            }

            saber.SetParent(handle);
            foreach (var collider in saber.GameObject.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }

        private void OnSelectedSaberChanged(string hash) => _ = Reload();

        private void OnHeldPreviewChanged(bool visible) => UpdateVisibility();

        private void UpdateVisibility()
        {
            var showSabers = _sabers != null && !SaberSelection.HeldPreviewVisible;
            _sabers?.SetActive(showSabers);

            // Custom Sabers Lite restores the handle renderers when its held preview closes.
            if (SaberSelection.HeldPreviewVisible)
            {
                return;
            }

            CollectHandleRenderers();
            foreach (var renderer in _handleRenderers)
            {
                renderer.enabled = !showSabers;
            }
        }

        private void CollectHandleRenderers()
        {
            _handleRenderers.RemoveAll(r => r == null);
            if (_handleRenderers.Count > 0)
            {
                return;
            }

            foreach (var handle in new[] { _leftHandle, _rightHandle })
            {
                if (handle == null)
                {
                    continue;
                }

                foreach (var name in HandleRendererNames)
                {
                    var child = handle.Find(name);
                    var renderer = child == null ? null : child.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        _handleRenderers.Add(renderer);
                    }
                }
            }
        }

        private void CancelLoading()
        {
            if (_loading == null)
            {
                return;
            }

            _loading.Cancel();
            _loading.Dispose();
            _loading = null;
        }

        private void DestroySabers()
        {
            _sabers?.Dispose();
            _sabers = null;
        }
    }
}
