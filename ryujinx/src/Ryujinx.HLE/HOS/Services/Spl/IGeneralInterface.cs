using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Services.Spl.Types;

namespace Ryujinx.HLE.HOS.Services.Spl
{
    [Service("spl:")]
    [Service("spl:es")]
    [Service("spl:fs")]
    [Service("spl:manu")]
    [Service("spl:mig")]
    [Service("spl:ssl")]
    class IGeneralInterface : IpcService
    {
        public IGeneralInterface(ServiceCtx context) { }

        [CommandCmif(0)]
        // GetConfig(u32 config_item) -> u64 config_value
        public ResultCode GetConfig(ServiceCtx context)
        {
            ConfigItem configItem = (ConfigItem)context.RequestData.ReadUInt32();

            // NOTE: Nintendo explicitly blacklists package2 hash here, amusingly.
            //       This is not blacklisted in safemode, but we're never in safe mode...
            if (configItem == ConfigItem.Package2Hash)
            {
                return ResultCode.InvalidArguments;
            }

            // TODO: This should call svcCallSecureMonitor using arg 0xC3000002.
            //       Since it's currently not implemented we can use a private method for now.
            SmcResult result = SmcGetConfig(context, out ulong configValue, configItem);

            // Nintendo has some special handling here for hardware type/is_retail.
            if (result == SmcResult.InvalidArgument)
            {
                switch (configItem)
                {
                    case ConfigItem.HardwareType:
                        configValue = (ulong)HardwareType.Icosa;
                        result = SmcResult.Success;
                        break;
                    case ConfigItem.HardwareState:
                        configValue = (ulong)HardwareState.Development;
                        result = SmcResult.Success;
                        break;
                    default:
                        break;
                }
            }

            context.ResponseData.Write(configValue);

            if(Ryujinx.Common.PlatformInfo.IsBionic)
            {
                if (result == SmcResult.Success)
                {
                    return ResultCode.Success;
                }
            }

            return (ResultCode)((int)result << 9) | ResultCode.ModuleId;
        }

        private SmcResult SmcGetConfig(ServiceCtx context, out ulong configValue, ConfigItem configItem)
        {
            configValue = default;

#pragma warning disable IDE0059 // Remove unnecessary value assignment
            SystemVersion version = context.Device.System.ContentManager.GetCurrentFirmwareVersion();
#pragma warning restore IDE0059
            MemorySize memorySize = context.Device.Configuration.MemoryConfiguration.ToKernelMemorySize();

            switch (configItem)
            {
                case ConfigItem.DisableProgramVerification:
                    configValue = 0;
                    break;
                case ConfigItem.DramId:
                    configValue = memorySize switch
                    {
                        MemorySize.MemorySize6GiB => (ulong)DramId.IcosaSamsung6GiB,
                        MemorySize.MemorySize8GiB => (ulong)DramId.IowaSamsung8GiB,
                        MemorySize.MemorySize10GiB => (ulong)DramId.IowaSamsung10GiB,
                        MemorySize.MemorySize12GiB => (ulong)DramId.IowaSamsung12GiB,
                        _ => (ulong)DramId.IcosaSamsung4GiB
                    };
                    break;
                case ConfigItem.SecurityEngineInterruptNumber:
                case ConfigItem.FuseVersion:
                    return SmcResult.NotImplemented;
                case ConfigItem.HardwareType:
                    configValue = memorySize switch
                    {
                        MemorySize.MemorySize6GiB => (ulong)HardwareType.Icosa,
                        MemorySize.MemorySize8GiB => (ulong)HardwareType.Iowa,
                        MemorySize.MemorySize10GiB => (ulong)HardwareType.Iowa,
                        MemorySize.MemorySize12GiB => (ulong)HardwareType.Iowa,
                        _ => (ulong)HardwareType.Icosa
                    };
                    break;
                case ConfigItem.HardwareState:
                    configValue = (ulong)HardwareState.Production;
                    break;
                case ConfigItem.IsRecoveryBoot:
                    configValue = 0;
                    break;
                case ConfigItem.DeviceId:
                    return SmcResult.NotImplemented;
                case ConfigItem.BootReason:
                    // This was removed in firmware 4.0.0.
                    return SmcResult.InvalidArgument;
                case ConfigItem.MemoryMode:
                    configValue = (ulong)context.Device.Configuration.MemoryConfiguration;
                    break;
                case ConfigItem.IsDevelopmentFunctionEnabled:
                    configValue = 0;
                    break;
                case ConfigItem.KernelConfiguration:
                case ConfigItem.IsChargerHiZModeEnabled:
                case ConfigItem.QuestState:
                case ConfigItem.RegulatorType:
                case ConfigItem.DeviceUniqueKeyGeneration:
                case ConfigItem.Package2Hash:
                    return SmcResult.NotImplemented;
                default:
                    return SmcResult.InvalidArgument;
            }

            return SmcResult.Success;
        }
    }
}
