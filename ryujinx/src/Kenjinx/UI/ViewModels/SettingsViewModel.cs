using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using LibHac.Tools.FsSystem;
using Ryujinx.Audio.Backends.OpenAL;
using Ryujinx.Audio.Backends.SDL2;
using Ryujinx.Audio.Backends.SoundIo;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.Models.Input;
using Ryujinx.Ava.UI.Windows;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Configuration.Multiplayer;
using Ryujinx.Common.GraphicsDriver;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Vulkan;
using Ryujinx.HLE;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using Ryujinx.UI.Common.Configuration;
using Ryujinx.UI.Common.Configuration.System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TimeZone = Ryujinx.Ava.UI.Models.TimeZone;

namespace Ryujinx.Ava.UI.ViewModels
{
    public partial class SettingsViewModel : BaseModel
    {
        private readonly VirtualFileSystem _virtualFileSystem;
        private readonly ContentManager _contentManager;
        private TimeZoneContentManager _timeZoneContentManager;

        private readonly List<string> _validTzRegions;

        private readonly Dictionary<string, string> _networkInterfaces;

        private float _customResolutionScale;
        private int _resolutionScale;
        private int _graphicsBackendMultithreadingIndex;
        private float _volume;
        private bool _isVulkanAvailable = true;
        private bool _gameDirectoryChanged;
        private bool _autoloadDirectoryChanged;
        private readonly List<string> _gpuIds = new();
        private int _graphicsBackendIndex;
        private int _scalingFilter;
        private int _scalingFilterLevel;
        private int _customVSyncInterval;
        private bool _enableCustomVSyncInterval;
        private int _customVSyncIntervalPercentageProxy;
        private VSyncMode _vSyncMode;

        public event Action CloseWindow;
        public event Action SaveSettingsEvent;
        private int _networkInterfaceIndex;
        private int _multiplayerModeIndex;
        private string _ldnPassphrase;
        private string _LdnServer;

        public int ResolutionScale
        {
            get => _resolutionScale;
            set
            {
                _resolutionScale = value;

                OnPropertyChanged(nameof(CustomResolutionScale));
                OnPropertyChanged(nameof(IsCustomResolutionScaleActive));
            }
        }

        public int GraphicsBackendMultithreadingIndex
        {
            get => _graphicsBackendMultithreadingIndex;
            set
            {
                _graphicsBackendMultithreadingIndex = value;

                if (_graphicsBackendMultithreadingIndex != (int)ConfigurationState.Instance.Graphics.BackendThreading.Value)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                         ContentDialogHelper.CreateInfoDialog(LocaleManager.Instance[LocaleKeys.DialogSettingsBackendThreadingWarningMessage],
                            "",
                            "",
                            LocaleManager.Instance[LocaleKeys.InputDialogOk],
                            LocaleManager.Instance[LocaleKeys.DialogSettingsBackendThreadingWarningTitle])
                    );
                }

                OnPropertyChanged();
            }
        }

        public float CustomResolutionScale
        {
            get => _customResolutionScale;
            set
            {
                _customResolutionScale = value;

                OnPropertyChanged();
            }
        }

        public bool IsVulkanAvailable
        {
            get => _isVulkanAvailable;
            set
            {
                _isVulkanAvailable = value;

                OnPropertyChanged();
            }
        }

        public bool IsOpenGLAvailable => !OperatingSystem.IsMacOS();

        public bool IsHypervisorAvailable => OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

        public bool GameDirectoryChanged
        {
            get => _gameDirectoryChanged;
            set
            {
                _gameDirectoryChanged = value;

                OnPropertyChanged();
            }
        }

        public bool AutoloadDirectoryChanged
        {
            get => _autoloadDirectoryChanged;
            set
            {
                _autoloadDirectoryChanged = value;

                OnPropertyChanged();
            }
        }

        public bool IsMacOS => OperatingSystem.IsMacOS();

        public bool EnableDiscordIntegration { get; set; }
        public bool CheckUpdatesOnStart { get; set; }
        public bool ShowConfirmExit { get; set; }
        public bool IgnoreApplet { get; set; }
        public bool RememberWindowState { get; set; }
        public int HideCursor { get; set; }
        public bool EnableDockedMode { get; set; }
        public bool EnableKeyboard { get; set; }
        public bool EnableMouse { get; set; }
        public VSyncMode VSyncMode
        {
            get => _vSyncMode;
            set
            {
                if (value == VSyncMode.Custom ||
                    value == VSyncMode.Switch ||
                    value == VSyncMode.Unbounded)
                {
                    _vSyncMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CustomVSyncIntervalPercentageProxy
        {
            get => _customVSyncIntervalPercentageProxy;
            set
            {
                int newInterval = (int)((value / 100f) * 60);
                _customVSyncInterval = newInterval;
                _customVSyncIntervalPercentageProxy = value;
                OnPropertyChanged((nameof(CustomVSyncInterval)));
                OnPropertyChanged((nameof(CustomVSyncIntervalPercentageText)));
            }
        }

        public string CustomVSyncIntervalPercentageText
        {
            get
            {
                string text = CustomVSyncIntervalPercentageProxy.ToString() + "%";
                return text;
            }
        }

        public bool EnableCustomVSyncInterval
        {
            get => _enableCustomVSyncInterval;
            set
            {
                _enableCustomVSyncInterval = value;
                if (_vSyncMode == VSyncMode.Custom && !value)
                {
                    VSyncMode = VSyncMode.Switch;
                }
                else if (value)
                {
                    VSyncMode = VSyncMode.Custom;
                }
                OnPropertyChanged();
            }
        }

        public int CustomVSyncInterval
        {
            get => _customVSyncInterval;
            set
            {
                _customVSyncInterval = value;
                int newPercent = (int)((value / 60f) * 100);
                _customVSyncIntervalPercentageProxy = newPercent;
                OnPropertyChanged(nameof(CustomVSyncIntervalPercentageProxy));
                OnPropertyChanged(nameof(CustomVSyncIntervalPercentageText));
                OnPropertyChanged();
            }
        }
        public bool EnablePptc { get; set; }
        public bool EnableLowPowerPptc { get; set; }

        public bool EnableJitCacheEviction { get; set; }
        public bool EnableInternetAccess { get; set; }
        public bool EnableFsIntegrityChecks { get; set; }
        public bool IgnoreMissingServices { get; set; }
        public MemoryConfiguration DramSize { get; set; }
        public bool EnableShaderCache { get; set; }
        public bool EnableTextureRecompression { get; set; }
        public bool EnableMacroHLE { get; set; }
        public bool EnableColorSpacePassthrough { get; set; }
        public bool ColorSpacePassthroughAvailable => IsMacOS;
        public bool EnableFileLog { get; set; }
        public bool EnableStub { get; set; }
        public bool EnableInfo { get; set; }
        public bool EnableWarn { get; set; }
        public bool EnableError { get; set; }
        public bool EnableTrace { get; set; }
        public bool EnableGuest { get; set; }
        public bool EnableFsAccessLog { get; set; }
        public bool EnableDebug { get; set; }
        public bool IsOpenAlEnabled { get; set; }
        public bool IsSoundIoEnabled { get; set; }
        public bool IsSDL2Enabled { get; set; }
        public bool IsCustomResolutionScaleActive => _resolutionScale == 4;
        public bool IsScalingFilterActive => _scalingFilter == (int)Ryujinx.Common.Configuration.ScalingFilter.Fsr;

        public bool IsVulkanSelected => GraphicsBackendIndex == 0;
        public bool UseHypervisor { get; set; }
        public bool DisableP2P { get; set; }

        public string TimeZone { get; set; }
        public string ShaderDumpPath { get; set; }

        public string LdnPassphrase
        {
            get => _ldnPassphrase;
            set
            {
                _ldnPassphrase = value;
                IsInvalidLdnPassphraseVisible = !ValidateLdnPassphrase(value);

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInvalidLdnPassphraseVisible));
            }
        }

        public int Language { get; set; }
        public int Region { get; set; }
        public int FsGlobalAccessLogMode { get; set; }
        public int AudioBackend { get; set; }
        public int MaxAnisotropy { get; set; }
        public int AspectRatio { get; set; }
        public int AntiAliasingEffect { get; set; }
        public string ScalingFilterLevelText => ScalingFilterLevel.ToString("0");
        public int ScalingFilterLevel
        {
            get => _scalingFilterLevel;
            set
            {
                _scalingFilterLevel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScalingFilterLevelText));
            }
        }
        public int OpenglDebugLevel { get; set; }
        public int MemoryMode { get; set; }
        public int BaseStyleIndex { get; set; }
        public int GraphicsBackendIndex
        {
            get => _graphicsBackendIndex;
            set
            {
                _graphicsBackendIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsVulkanSelected));
            }
        }
        public int ScalingFilter
        {
            get => _scalingFilter;
            set
            {
                _scalingFilter = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsScalingFilterActive));
            }
        }

        public int PreferredGpuIndex { get; set; }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;

                ConfigurationState.Instance.System.AudioVolume.Value = _volume / 100;

                OnPropertyChanged();
            }
        }

        //private DateTimeOffset _currentDate;
        //private TimeSpan _currentTime;

        public bool MatchSystemTime { get; set; }

        public DateTimeOffset CurrentDate { get; set; }

        public TimeSpan CurrentTime { get; set; }

        internal AvaloniaList<TimeZone> TimeZones { get; set; }
        public AvaloniaList<string> GameDirectories { get; set; }
        public AvaloniaList<string> AutoloadDirectories { get; set; }
        public ObservableCollection<ComboBoxItem> AvailableGpus { get; set; }

        public AvaloniaList<string> NetworkInterfaceList
        {
            get => new(_networkInterfaces.Keys);
        }

        public HotkeyConfig KeyboardHotkey { get; set; }

        public int NetworkInterfaceIndex
        {
            get => _networkInterfaceIndex;
            set
            {
                _networkInterfaceIndex = value != -1 ? value : 0;
                ConfigurationState.Instance.Multiplayer.LanInterfaceId.Value = _networkInterfaces[NetworkInterfaceList[_networkInterfaceIndex]];
            }
        }

        public int MultiplayerModeIndex
        {
            get => _multiplayerModeIndex;
            set
            {
                _multiplayerModeIndex = value;
                ConfigurationState.Instance.Multiplayer.Mode.Value = (MultiplayerMode)_multiplayerModeIndex;
            }
        }

        [GeneratedRegex("Kenji-NX-[0-9a-f]{8}")]
        private static partial Regex LdnPassphraseRegex();

        public bool IsInvalidLdnPassphraseVisible { get; set; }

        public string LdnServer
        {
            get => _LdnServer;
            set
            {
                _LdnServer = value;
                OnPropertyChanged();
            }
        }

        public SettingsViewModel(VirtualFileSystem virtualFileSystem, ContentManager contentManager) : this()
        {
            _virtualFileSystem = virtualFileSystem;
            _contentManager = contentManager;
            if (Program.PreviewerDetached)
            {
                Task.Run(LoadTimeZones);
            }
        }

        public SettingsViewModel()
        {
            GameDirectories = [];
            AutoloadDirectories = [];
            TimeZones = [];
            AvailableGpus = [];
            _validTzRegions = [];
            _networkInterfaces = new Dictionary<string, string>();

            Task.Run(CheckSoundBackends);
            Task.Run(PopulateNetworkInterfaces);

            if (Program.PreviewerDetached)
            {
                Task.Run(LoadAvailableGpus);
                LoadCurrentConfiguration();
            }
        }

        public async Task CheckSoundBackends()
        {
            IsOpenAlEnabled = OpenALHardwareDeviceDriver.IsSupported;
            IsSoundIoEnabled = SoundIoHardwareDeviceDriver.IsSupported;
            IsSDL2Enabled = SDL2HardwareDeviceDriver.IsSupported;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                OnPropertyChanged(nameof(IsOpenAlEnabled));
                OnPropertyChanged(nameof(IsSoundIoEnabled));
                OnPropertyChanged(nameof(IsSDL2Enabled));
            });
        }

        private async Task LoadAvailableGpus()
        {
            AvailableGpus.Clear();

            var devices = VulkanRenderer.GetPhysicalDevices();

            if (devices.Length == 0)
            {
                IsVulkanAvailable = false;
                GraphicsBackendIndex = 1;
            }
            else
            {
                foreach (var device in devices)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _gpuIds.Add(device.Id);

                        AvailableGpus.Add(new ComboBoxItem { Content = $"{device.Name} {(device.IsDiscrete ? "(dGPU)" : "")}" });
                    });
                }
            }

            // GPU configuration needs to be loaded during the async method or it will always return 0.
            PreferredGpuIndex = _gpuIds.Contains(ConfigurationState.Instance.Graphics.PreferredGpu) ?
                                _gpuIds.IndexOf(ConfigurationState.Instance.Graphics.PreferredGpu) : 0;

            Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(PreferredGpuIndex)));
        }

        public async Task LoadTimeZones()
        {
            _timeZoneContentManager = new TimeZoneContentManager();

            _timeZoneContentManager.InitializeInstance(_virtualFileSystem, _contentManager, IntegrityCheckLevel.None);

            foreach ((int offset, string location, string abbr) in _timeZoneContentManager.ParseTzOffsets())
            {
                int hours = Math.DivRem(offset, 3600, out int seconds);
                int minutes = Math.Abs(seconds) / 60;

                string abbr2 = abbr.StartsWith('+') || abbr.StartsWith('-') ? string.Empty : abbr;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TimeZones.Add(new TimeZone($"UTC{hours:+0#;-0#;+00}:{minutes:D2}", location, abbr2));

                    _validTzRegions.Add(location);
                });
            }

            Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(TimeZone)));
        }

        private async Task PopulateNetworkInterfaces()
        {
            _networkInterfaces.Clear();
            _networkInterfaces.Add(LocaleManager.Instance[LocaleKeys.NetworkInterfaceDefault], "0");

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _networkInterfaces.Add(networkInterface.Name, networkInterface.Id);
                });
            }

            // Network interface index  needs to be loaded during the async method or it will always return 0.
            NetworkInterfaceIndex = _networkInterfaces.Values.ToList().IndexOf(ConfigurationState.Instance.Multiplayer.LanInterfaceId.Value);

            Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(NetworkInterfaceIndex)));
        }

        private bool ValidateLdnPassphrase(string passphrase)
        {
            return string.IsNullOrEmpty(passphrase) || (passphrase.Length == 16 && LdnPassphraseRegex().IsMatch(passphrase));
        }

        public void ValidateAndSetTimeZone(string location)
        {
            if (_validTzRegions.Contains(location))
            {
                TimeZone = location;
            }
        }

        public void LoadCurrentConfiguration()
        {
            ConfigurationState config = ConfigurationState.Instance;

            // User Interface
            EnableDiscordIntegration = config.EnableDiscordIntegration;
            CheckUpdatesOnStart = config.CheckUpdatesOnStart;
            ShowConfirmExit = config.ShowConfirmExit;
            IgnoreApplet = config.IgnoreControllerApplet;
            RememberWindowState = config.RememberWindowState;
            HideCursor = (int)config.HideCursor.Value;

            GameDirectories.Clear();
            GameDirectories.AddRange(config.UI.GameDirs.Value);

            AutoloadDirectories.Clear();
            AutoloadDirectories.AddRange(config.UI.AutoloadDirs.Value);

            BaseStyleIndex = config.UI.BaseStyle.Value switch
            {
                "Auto" => 0,
                "Light" => 1,
                "Dark" => 2,
                _ => 0
            };

            // Input
            EnableDockedMode = config.System.EnableDockedMode;
            EnableKeyboard = config.Hid.EnableKeyboard;
            EnableMouse = config.Hid.EnableMouse;

            // Keyboard Hotkeys
            KeyboardHotkey = new HotkeyConfig(config.Hid.Hotkeys.Value);

            // System
            Region = (int)config.System.Region.Value;
            Language = (int)config.System.Language.Value;
            TimeZone = config.System.TimeZone;

            DateTime currentHostDateTime = DateTime.Now;
            TimeSpan systemDateTimeOffset = TimeSpan.FromSeconds(config.System.SystemTimeOffset);
            DateTime currentDateTime = currentHostDateTime.Add(systemDateTimeOffset);
            CurrentDate = currentDateTime.Date;
            CurrentTime = currentDateTime.TimeOfDay;

            MatchSystemTime = config.System.MatchSystemTime;

            EnableCustomVSyncInterval = config.Graphics.EnableCustomVSyncInterval;
            CustomVSyncInterval = config.Graphics.CustomVSyncInterval;
            VSyncMode = config.Graphics.VSyncMode;
            EnableFsIntegrityChecks = config.System.EnableFsIntegrityChecks;
            DramSize = config.System.DramSize;
            IgnoreMissingServices = config.System.IgnoreMissingServices;

            // CPU
            EnablePptc = config.System.EnablePtc;
            EnableLowPowerPptc = config.System.EnableLowPowerPtc;
            EnableJitCacheEviction = config.System.EnableJitCacheEviction;
            MemoryMode = (int)config.System.MemoryManagerMode.Value;
            UseHypervisor = config.System.UseHypervisor;

            // Graphics
            GraphicsBackendIndex = (int)config.Graphics.GraphicsBackend.Value;
            // Physical devices are queried asynchronously hence the preferred index config value is loaded in LoadAvailableGpus().
            EnableShaderCache = config.Graphics.EnableShaderCache;
            EnableTextureRecompression = config.Graphics.EnableTextureRecompression;
            EnableMacroHLE = config.Graphics.EnableMacroHLE;
            EnableColorSpacePassthrough = config.Graphics.EnableColorSpacePassthrough;
            ResolutionScale = config.Graphics.ResScale == -1 ? 4 : config.Graphics.ResScale - 1;
            CustomResolutionScale = config.Graphics.ResScaleCustom;
            MaxAnisotropy = config.Graphics.MaxAnisotropy == -1 ? 0 : (int)(MathF.Log2(config.Graphics.MaxAnisotropy));
            AspectRatio = (int)config.Graphics.AspectRatio.Value;
            GraphicsBackendMultithreadingIndex = (int)config.Graphics.BackendThreading.Value;
            ShaderDumpPath = config.Graphics.ShadersDumpPath;
            AntiAliasingEffect = (int)config.Graphics.AntiAliasing.Value;
            ScalingFilter = (int)config.Graphics.ScalingFilter.Value;
            ScalingFilterLevel = config.Graphics.ScalingFilterLevel.Value;

            // Audio
            AudioBackend = (int)config.System.AudioBackend.Value;
            Volume = config.System.AudioVolume * 100;

            // Network
            EnableInternetAccess = config.System.EnableInternetAccess;
            // LAN interface index is loaded asynchronously in PopulateNetworkInterfaces()

            // Logging
            EnableFileLog = config.Logger.EnableFileLog;
            EnableStub = config.Logger.EnableStub;
            EnableInfo = config.Logger.EnableInfo;
            EnableWarn = config.Logger.EnableWarn;
            EnableError = config.Logger.EnableError;
            EnableTrace = config.Logger.EnableTrace;
            EnableGuest = config.Logger.EnableGuest;
            EnableDebug = config.Logger.EnableDebug;
            EnableFsAccessLog = config.Logger.EnableFsAccessLog;
            FsGlobalAccessLogMode = config.System.FsGlobalAccessLogMode;
            OpenglDebugLevel = (int)config.Logger.GraphicsDebugLevel.Value;

            MultiplayerModeIndex = (int)config.Multiplayer.Mode.Value;
            DisableP2P = config.Multiplayer.DisableP2p.Value;
            LdnPassphrase = config.Multiplayer.LdnPassphrase.Value;
            LdnServer = config.Multiplayer.LdnServer.Value;
        }

        public void SaveSettings()
        {
            ConfigurationState config = ConfigurationState.Instance;

            // User Interface
            config.EnableDiscordIntegration.Value = EnableDiscordIntegration;
            config.CheckUpdatesOnStart.Value = CheckUpdatesOnStart;
            config.ShowConfirmExit.Value = ShowConfirmExit;
            config.IgnoreControllerApplet.Value = IgnoreApplet;
            config.RememberWindowState.Value = RememberWindowState;
            config.HideCursor.Value = (HideCursorMode)HideCursor;

            if (_gameDirectoryChanged)
            {
                List<string> gameDirs = new(GameDirectories);
                config.UI.GameDirs.Value = gameDirs;
            }

            if (_autoloadDirectoryChanged)
            {
                List<string> autoloadDirs = new(AutoloadDirectories);
                config.UI.AutoloadDirs.Value = autoloadDirs;
            }

            config.UI.BaseStyle.Value = BaseStyleIndex switch
            {
                0 => "Auto",
                1 => "Light",
                2 => "Dark",
                _ => "Auto"
            };

            // Input
            config.System.EnableDockedMode.Value = EnableDockedMode;
            config.Hid.EnableKeyboard.Value = EnableKeyboard;
            config.Hid.EnableMouse.Value = EnableMouse;

            // Keyboard Hotkeys
            config.Hid.Hotkeys.Value = KeyboardHotkey.GetConfig();

            // System
            config.System.Region.Value = (Region)Region;
            config.System.Language.Value = (Language)Language;

            if (_validTzRegions.Contains(TimeZone))
            {
                config.System.TimeZone.Value = TimeZone;
            }

            config.System.MatchSystemTime.Value = MatchSystemTime;
            config.System.SystemTimeOffset.Value = Convert.ToInt64((CurrentDate.ToUnixTimeSeconds() + CurrentTime.TotalSeconds) - DateTimeOffset.Now.ToUnixTimeSeconds());
            config.Graphics.VSyncMode.Value = VSyncMode;
            config.Graphics.EnableCustomVSyncInterval.Value = EnableCustomVSyncInterval;
            config.Graphics.CustomVSyncInterval.Value = CustomVSyncInterval;
            config.System.EnableFsIntegrityChecks.Value = EnableFsIntegrityChecks;
            config.System.DramSize.Value = DramSize;
            config.System.IgnoreMissingServices.Value = IgnoreMissingServices;

            // CPU
            config.System.EnablePtc.Value = EnablePptc;
            config.System.EnableLowPowerPtc.Value = EnableLowPowerPptc;
            config.System.EnableJitCacheEviction.Value = EnableJitCacheEviction;
            config.System.MemoryManagerMode.Value = (MemoryManagerMode)MemoryMode;
            config.System.UseHypervisor.Value = UseHypervisor;

            // Graphics
            config.Graphics.GraphicsBackend.Value = (GraphicsBackend)GraphicsBackendIndex;
            config.Graphics.PreferredGpu.Value = _gpuIds.ElementAtOrDefault(PreferredGpuIndex);
            config.Graphics.EnableShaderCache.Value = EnableShaderCache;
            config.Graphics.EnableTextureRecompression.Value = EnableTextureRecompression;
            config.Graphics.EnableMacroHLE.Value = EnableMacroHLE;
            config.Graphics.EnableColorSpacePassthrough.Value = EnableColorSpacePassthrough;
            config.Graphics.ResScale.Value = ResolutionScale == 4 ? -1 : ResolutionScale + 1;
            config.Graphics.ResScaleCustom.Value = CustomResolutionScale;
            config.Graphics.MaxAnisotropy.Value = MaxAnisotropy == 0 ? -1 : MathF.Pow(2, MaxAnisotropy);
            config.Graphics.AspectRatio.Value = (AspectRatio)AspectRatio;
            config.Graphics.AntiAliasing.Value = (AntiAliasing)AntiAliasingEffect;
            config.Graphics.ScalingFilter.Value = (ScalingFilter)ScalingFilter;
            config.Graphics.ScalingFilterLevel.Value = ScalingFilterLevel;

            if (ConfigurationState.Instance.Graphics.BackendThreading != (BackendThreading)GraphicsBackendMultithreadingIndex)
            {
                DriverUtilities.ToggleOGLThreading(GraphicsBackendMultithreadingIndex == (int)BackendThreading.Off);
            }

            config.Graphics.BackendThreading.Value = (BackendThreading)GraphicsBackendMultithreadingIndex;
            config.Graphics.ShadersDumpPath.Value = ShaderDumpPath;

            // Audio
            AudioBackend audioBackend = (AudioBackend)AudioBackend;
            if (audioBackend != config.System.AudioBackend.Value)
            {
                config.System.AudioBackend.Value = audioBackend;

                Logger.Info?.Print(LogClass.Application, $"AudioBackend toggled to: {audioBackend}");
            }

            config.System.AudioVolume.Value = Volume / 100;

            // Network
            config.System.EnableInternetAccess.Value = EnableInternetAccess;

            // Logging
            config.Logger.EnableFileLog.Value = EnableFileLog;
            config.Logger.EnableStub.Value = EnableStub;
            config.Logger.EnableInfo.Value = EnableInfo;
            config.Logger.EnableWarn.Value = EnableWarn;
            config.Logger.EnableError.Value = EnableError;
            config.Logger.EnableTrace.Value = EnableTrace;
            config.Logger.EnableGuest.Value = EnableGuest;
            config.Logger.EnableDebug.Value = EnableDebug;
            config.Logger.EnableFsAccessLog.Value = EnableFsAccessLog;
            config.System.FsGlobalAccessLogMode.Value = FsGlobalAccessLogMode;
            config.Logger.GraphicsDebugLevel.Value = (GraphicsDebugLevel)OpenglDebugLevel;

            config.Multiplayer.LanInterfaceId.Value = _networkInterfaces[NetworkInterfaceList[NetworkInterfaceIndex]];
            config.Multiplayer.Mode.Value = (MultiplayerMode)MultiplayerModeIndex;
            config.Multiplayer.DisableP2p.Value = DisableP2P;
            config.Multiplayer.LdnPassphrase.Value = LdnPassphrase;
            config.Multiplayer.LdnServer.Value = LdnServer;

            config.ToFileFormat().SaveConfig(Program.ConfigurationPath);

            MainWindow.UpdateGraphicsConfig();
            MainWindow.MainWindowViewModel.VSyncModeSettingChanged();

            SaveSettingsEvent?.Invoke();

            _gameDirectoryChanged = false;
            _autoloadDirectoryChanged = false;
        }

        private static void RevertIfNotSaved()
        {
            Program.ReloadConfig();
        }

        public void ApplyButton()
        {
            SaveSettings();
        }

        public void OkButton()
        {
            SaveSettings();
            CloseWindow?.Invoke();
        }

        public void CancelButton()
        {
            RevertIfNotSaved();
            CloseWindow?.Invoke();
        }
    }
}
