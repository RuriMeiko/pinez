package org.kenjinx.android

import com.sun.jna.JNIEnv
import com.sun.jna.Library
import com.sun.jna.Native
import org.kenjinx.android.viewmodels.GameInfo
import java.util.Collections

interface KenjinxNativeJna : Library {
    fun deviceInitialize(
        memoryManagerMode: Int,
        useNce: Boolean,
        memoryConfiguration: Int,
        systemLanguage: Int,
        regionCode: Int,
        vSyncMode: Int,
        enableDockedMode: Boolean,
        enablePptc: Boolean,
        enableLowPowerPptc: Boolean,
        enableJitCacheEviction: Boolean,
        enableInternetAccess: Boolean,
        enableFsIntegrityChecks: Boolean,
        fsGlobalAccessLogMode: Int,
        timeZone: String,
        ignoreMissingServices: Boolean
    ): Boolean

    fun graphicsInitialize(
        rescale: Float = 1f,
        maxAnisotropy: Float = 0f,
        fastGpuTime: Boolean = true,
        fast2DCopy: Boolean = true,
        enableMacroJit: Boolean = false,
        enableMacroHLE: Boolean = true,
        enableShaderCache: Boolean = true,
        enableTextureRecompression: Boolean = false,
        backendThreading: Int = BackendThreading.Auto.ordinal
    ): Boolean

    fun graphicsInitializeRenderer(
        extensions: Array<String>,
        extensionsLength: Int,
        driver: Long
    ): Boolean

    fun javaInitialize(appPath: String, env: JNIEnv): Boolean
    fun deviceLaunchMiiEditor(): Boolean
    fun deviceGetGameFrameRate(): Double
    fun deviceGetGameFrameTime(): Double
    fun deviceGetGameFifo(): Double
    fun deviceLoadDescriptor(fileDescriptor: Int, gameType: Int, updateDescriptor: Int): Boolean
    fun graphicsRendererSetSize(width: Int, height: Int)
    fun graphicsRendererSetVsync(vSyncMode: Int)
    fun graphicsRendererRunLoop()
    fun deviceReloadFilesystem()
    fun inputInitialize(width: Int, height: Int)
    fun inputSetClientSize(width: Int, height: Int)
    fun inputSetTouchPoint(x: Int, y: Int)
    fun inputReleaseTouchPoint()
    fun inputUpdate()
    fun inputSetButtonPressed(button: Int, id: Int)
    fun inputSetButtonReleased(button: Int, id: Int)
    fun inputConnectGamepad(index: Int): Int
    fun inputSetStickAxis(stick: Int, x: Float, y: Float, id: Int)
    fun inputSetAccelerometerData(x: Float, y: Float, z: Float, id: Int)
    fun inputSetGyroData(x: Float, y: Float, z: Float, id: Int)
    fun deviceCloseEmulation()
    fun deviceReinitEmulation()
    fun deviceSignalEmulationClose()
    fun userGetOpenedUser(): String
    fun userGetUserPicture(userId: String): String
    fun userSetUserPicture(userId: String, picture: String)
    fun userGetUserName(userId: String): String
    fun userSetUserName(userId: String, userName: String)
    fun userAddUser(username: String, picture: String)
    fun userDeleteUser(userId: String)
    fun userOpenUser(userId: String)
    fun userCloseUser(userId: String)
    fun loggingSetEnabled(logLevel: Int, enabled: Boolean)
    fun deviceVerifyFirmware(fileDescriptor: Int, isXci: Boolean): String
    fun deviceInstallFirmware(fileDescriptor: Int, isXci: Boolean)
    fun deviceGetInstalledFirmwareVersion(): String
    fun uiHandlerSetup()
    fun uiHandlerSetResponse(isOkPressed: Boolean, input: String)
    fun deviceGetDlcTitleId(path: String, ncaPath: String): String
    fun deviceGetGameInfo(fileDescriptor: Int, extension: String, info: GameInfo)
    fun userGetAllUsers(): Array<String>
    fun deviceGetDlcContentList(path: String, titleId: Long): Array<String>
    fun loggingEnabledGraphicsLog(enabled: Boolean)
}

val jnaInstance: KenjinxNativeJna = Native.load(
    "kenjinx",
    KenjinxNativeJna::class.java,
    Collections.singletonMap(Library.OPTION_ALLOW_OBJECTS, true)
)

object KenjinxNative : KenjinxNativeJna by jnaInstance {

    fun loggingSetEnabled(logLevel: LogLevel, enabled: Boolean) = loggingSetEnabled(logLevel.ordinal, enabled)

    @JvmStatic
    fun frameEnded() = MainActivity.frameEnded()

    @JvmStatic
    fun getSurfacePtr(): Long = MainActivity.mainViewModel?.gameHost?.currentSurface ?: -1

    @JvmStatic
    fun getWindowHandle(): Long =
        MainActivity.mainViewModel?.gameHost?.currentWindowHandle ?: -1

    @JvmStatic
    fun updateProgress(infoPtr: Long, progress: Float) =
        MainActivity.mainViewModel?.gameHost?.setProgress(
            NativeHelpers.instance.getStringJava(infoPtr),
            progress
        )

    @JvmStatic
    fun updateUiHandler(
        newTitlePointer: Long,
        newMessagePointer: Long,
        newWatermarkPointer: Long,
        newType: Int,
        min: Int,
        max: Int,
        nMode: Int,
        newSubtitlePointer: Long,
        newInitialTextPointer: Long
    ) = MainActivity.mainViewModel?.activity?.uiHandler?.update(
        newTitle = NativeHelpers.instance.getStringJava(newTitlePointer),
        newMessage = NativeHelpers.instance.getStringJava(newMessagePointer),
        newWatermark = NativeHelpers.instance.getStringJava(newWatermarkPointer),
        newType,
        min,
        max,
        newMode = KeyboardMode.entries[nMode],
        newSubtitle = NativeHelpers.instance.getStringJava(newSubtitlePointer),
        NativeHelpers.instance.getStringJava(newInitialTextPointer)
    )
}
