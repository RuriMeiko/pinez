using Ryujinx.Audio.Backends.CompatLayer;
using Ryujinx.Audio.Integration;
using Ryujinx.Common.Configuration;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Apm;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.Loaders.Processes;
using Ryujinx.HLE.UI;
using Ryujinx.Memory;
using System;
using System.IO;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        public HLEConfiguration Configuration { get; }
        public IHardwareDeviceDriver AudioDeviceDriver { get; }
        public MemoryBlock Memory { get; }
        public GpuContext Gpu { get; }
        public VirtualFileSystem FileSystem { get; }
        public HOS.Horizon System { get; }
        public ProcessLoader Processes { get; }
        public PerformanceStatistics Statistics { get; }
        public Hid Hid { get; }
        public TamperMachine TamperMachine { get; }
        public IHostUIHandler UIHandler { get; }

        public int CpuCoresCount = 4; //Switch 1 has 4 cores

        public VSyncMode VSyncMode { get; set; } = VSyncMode.Switch;
        public bool CustomVSyncIntervalEnabled { get; set; } = false;
        public int CustomVSyncInterval { get; set; }

        public long TargetVSyncInterval { get; set; } = 60;

        public bool IsFrameAvailable => Gpu.Window.IsFrameAvailable;

        public Switch(HLEConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration.GpuRenderer);
            ArgumentNullException.ThrowIfNull(configuration.AudioDeviceDriver);
            ArgumentNullException.ThrowIfNull(configuration.UserChannelPersistence);

            Configuration = configuration;
            FileSystem = Configuration.VirtualFileSystem;
            UIHandler = Configuration.HostUIHandler;

            MemoryAllocationFlags memoryAllocationFlags = configuration.MemoryManagerMode == MemoryManagerMode.SoftwarePageTable
                ? MemoryAllocationFlags.Reserve
                : MemoryAllocationFlags.Reserve | MemoryAllocationFlags.Mirrorable;

#pragma warning disable IDE0055 // Disable formatting
            AudioDeviceDriver = new CompatLayerHardwareDeviceDriver(Configuration.AudioDeviceDriver);
            Memory            = new MemoryBlock(Configuration.MemoryConfiguration.ToDramSize(), memoryAllocationFlags);
            Gpu               = new GpuContext(Configuration.GpuRenderer);
            System            = new HOS.Horizon(this);
            Statistics        = new PerformanceStatistics();
            Hid               = new Hid(this, System.HidStorage);
            Processes         = new ProcessLoader(this);
            TamperMachine     = new TamperMachine();

            System.InitializeServices();
            System.State.SetLanguage(Configuration.SystemLanguage);
            System.State.SetRegion(Configuration.Region);

            VSyncMode                               = Configuration.VSyncMode;
            CustomVSyncInterval                     = Configuration.CustomVSyncInterval;
            System.State.DockedMode                 = Configuration.EnableDockedMode;
            System.PerformanceState.PerformanceMode = System.State.DockedMode ? PerformanceMode.Boost : PerformanceMode.Default;
            System.EnablePtc                        = Configuration.EnablePtc;
            System.FsIntegrityCheckLevel            = Configuration.FsIntegrityCheckLevel;
            System.GlobalAccessLogMode              = Configuration.FsGlobalAccessLogMode;
            UpdateVSyncInterval();
#pragma warning restore IDE0055
        }

        public bool LoadCart(string exeFsDir, string romFsFile = null)
        {
            return Processes.LoadUnpackedNca(exeFsDir, romFsFile);
        }

        public bool LoadXci(string xciFile, ulong applicationId = 0)
        {
            return Processes.LoadXci(xciFile, applicationId);
        }

        public bool LoadNca(string ncaFile)
        {
            return Processes.LoadNca(ncaFile);
        }

        public bool LoadNsp(string nspFile, ulong applicationId = 0)
        {
            return Processes.LoadNsp(nspFile, applicationId);
        }

        public bool LoadProgram(string fileName)
        {
            return Processes.LoadNxo(fileName);
        }

        public bool LoadXci(Stream xciStream, ulong applicationId = 0, Stream updateStream = null)
        {
            return Processes.LoadXci(xciStream, applicationId, updateStream);
        }

        public bool LoadNca(Stream ncaStream)
        {
            return Processes.LoadNca(ncaStream);
        }

        public bool LoadNsp(Stream nspStream, ulong applicationId = 0, Stream updateStream = null)
        {
            return Processes.LoadNsp(nspStream, applicationId, updateStream);
        }

        public bool LoadProgram(Stream stream, bool isNro, string name)
        {
            return Processes.LoadNxo(stream, isNro, name);
        }

        public bool WaitFifo()
        {
            return Gpu.GPFifo.WaitForCommands();
        }

        public void ProcessFrame()
        {
            Gpu.ProcessShaderCacheQueue();
            Gpu.Renderer.PreFrame();
            Gpu.GPFifo.DispatchCalls();
        }

        public bool ConsumeFrameAvailable()
        {
            return Gpu.Window.ConsumeFrameAvailable();
        }

        public void PresentFrame(Action swapBuffersCallback)
        {
            Gpu.Window.Present(swapBuffersCallback);
        }

        public void IncrementCustomVSyncInterval()
        {
            CustomVSyncInterval += 1;
            UpdateVSyncInterval();
        }

        public void DecrementCustomVSyncInterval()
        {
            CustomVSyncInterval -= 1;
            UpdateVSyncInterval();
        }

        public void UpdateVSyncInterval()
        {
            switch (VSyncMode)
            {
                case VSyncMode.Custom:
                    TargetVSyncInterval = CustomVSyncInterval;
                    break;
                case VSyncMode.Switch:
                    TargetVSyncInterval = 60;
                    break;
                case VSyncMode.Unbounded:
                    TargetVSyncInterval = 1;
                    break;
            }
        }

        public void SetVolume(float volume)
        {
            AudioDeviceDriver.Volume = Math.Clamp(volume, 0f, 1f);
        }

        public float GetVolume()
        {
            return AudioDeviceDriver.Volume;
        }

        public void EnableCheats()
        {
            ModLoader.EnableCheats(Processes.ActiveApplication.ProgramId, TamperMachine);
        }

        public bool IsAudioMuted()
        {
            return AudioDeviceDriver.Volume == 0;
        }

        public void DisposeGpu()
        {
            Gpu.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                System.Dispose();
                AudioDeviceDriver.Dispose();
                FileSystem.Dispose();
                Memory.Dispose();
            }
        }
    }
}
