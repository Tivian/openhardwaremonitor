/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2019 Paweł Kania
  Based on https://docs.nvidia.com/deploy/nvml-api/
 
*/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenHardwareMonitor.Hardware.Nvidia {
  internal enum NvmlReturn {
    SUCCESS = 0,             // The operation was successful
    UNINITIALIZED = 1,       // NVML was not first initialized with nvmlInit()
    INVALID_ARGUMENT = 2,    // A supplied argument is invalid
    NOT_SUPPORTED = 3,       // The requested operation is not available on target device
    NO_PERMISSION = 4,       // The current user does not have permission for operation
    ALREADY_INITIALIZED = 5, // Deprecated: Multiple initializations are now allowed through ref counting
    NOT_FOUND = 6,           // A query to find an object was unsuccessful
    INSUFFICIENT_SIZE = 7,   // An input argument is not large enough
    INSUFFICIENT_POWER = 8,  // A device's external power cables are not properly attached
    DRIVER_NOT_LOADED = 9,   // NVIDIA driver is not loaded
    TIMEOUT = 10,            // User provided timeout passed
    IRQ_ISSUE = 11,          // NVIDIA Kernel detected an interrupt issue with a GPU
    LIBRARY_NOT_FOUND = 12,  // NVML Shared Library couldn't be found or loaded
    FUNCTION_NOT_FOUND = 13, // Local version of NVML doesn't implement this function
    CORRUPTED_INFOROM = 14,  // infoROM is corrupted
    GPU_IS_LOST = 15,        // The GPU has fallen off the bus or has otherwise become inaccessible
    RESET_REQUIRED = 16,     // The GPU requires a reset before it can be used again
    UNKNOWN = 999            // An internal driver error occurred
  }

  internal enum NvmlBrandType {
    UNKNOWN = 0,
    QUADRO = 1,
    TESLA = 2,
    NVS = 3,
    GRID = 4,
    GEFORCE = 5
  }

  internal enum NvmlClockType {
    GRAPHICS = 0, // Graphics clock domain
    SM = 1,       // SM clock domain
    MEM = 2,      // Memory clock domain
    VIDEO = 3
  }

  internal enum NvmlComputeMode {
    DEFAULT = 0,          // Default compute mode -- multiple contexts per device
    EXCLUSIVE_THREAD = 1, // Support Removed
    PROHIBITED = 2,       // Compute-prohibited mode -- no contexts per device
    EXCLUSIVE_PROCESS = 3 // Compute-exclusive-process mode -- only one context per device, usable from multiple threads at a time
  }

  internal enum NvmlDriverModel {
    WDDM = 0, // WDDM driver model -- GPU treated as a display device
    WDM = 1   // WDM (TCC) model (recommended) -- GPU treated as a generic device
  }

  internal enum NvmlEccCounterType {
    VOLATILE_ECC = 0, // Volatile counts are reset each time the driver loads
    AGGREGATE_ECC = 1 // Aggregate counts persist across reboots (i.e. for the lifetime of the device). 
  }

  internal enum NvmlEnableState {
    FEATURE_DISABLED = 0,
    FEATURE_ENABLED = 1
  }

  internal enum NvmlGpuOperationMode {
    ALL_ON = 0,   // Everything is enabled and running at full speed
    COMPUTE = 1,  // Designed for running only compute tasks. Graphics operations are not allowed
    LOW_DP = 2    // Designed for running graphics applications that don't require high bandwidth double precision 
  }

  internal enum NvmlInforomObject {
    OEM = 0,  // An object defined by OEM
    ECC = 1,  // The ECC object determining the level of ECC support
    POWER = 2 // The power management object
  }

  internal enum NvmlMemoryErrorType {
    CORRECTED = 0,  // A memory error that was correctedFor ECC errors,
                    //  these are single bit errors For Texture memory,
                    //  these are errors fixed by resend
    UNCORRECTED = 1 // A memory error that was not correctedFor ECC errors,
                    //  these are double bit errors For Texture memory,
                    //  these are errors where the resend fails
  }

  internal enum NvmlMemoryLocation {
    L1_CACHE = 0,
    L2_CACHE = 1,
    DEVICE_MEMORY = 2,
    REGISTER_FILE = 3,
    TEXTURE_MEMORY = 4
  }

  internal enum NvmlPageRetirementCause {
    MULTIPLE_SINGLE_BIT_ECC_ERRORS = 0, // Page was retired due to multiple single bit ECC error
    DOUBLE_BIT_ECC_ERROR = 1 // Page was retired due to double bit ECC error. 
  }

  internal enum NvmlPstates {
    PSTATE_0 = 0, // Performance state 0 -- Maximum Performance 
    PSTATE_1 = 1,
    PSTATE_2 = 2,
    PSTATE_3 = 3,
    PSTATE_4 = 4,
    PSTATE_5 = 5,
    PSTATE_6 = 6,
    PSTATE_7 = 7,
    PSTATE_8 = 8,
    PSTATE_9 = 9,
    PSTATE_10 = 10,
    PSTATE_11 = 11,
    PSTATE_12 = 12,
    PSTATE_13 = 13,
    PSTATE_14 = 14,
    PSTATE_15 = 15, // Performance state 15 -- Minimum Performance
    PSTATE_UNKNOWN = 32 // Unknown performance state
  }

  internal enum NvmlRestrictedAPI {
    SET_APPLICATION_CLOCKS = 0, // APIs that change application clocks
    SET_AUTO_BOOSTED_CLOCKS = 1 // APIs that enable/disable auto boosted clocks
  }

  internal enum NvmlTemperatureSensors {
    GPU = 0 // Temperature sensor for the GPU die
  }

  internal enum NvmlTemperatureThresholds {
    SHUTDOWN = 0,
    SLOWDOWN = 1
  }

  internal enum NvmlBridgeChipType {
    PLX = 0,
    BRO4 = 1
  }

  internal enum NvmlGpuTopologyLevel {
    INTERNAL = 0,
    SINGLE = 10,
    MULTIPLE = 20,
    HOSTBRIDGE = 30,
    CPU = 40,
    SYSTEM = 50
  }

  internal enum NvmlPcieUtilCounter {
    TX_BYTES = 0,
    RX_BYTES = 1
  }

  internal enum NvmlPerfPolicyType {
    POWER = 0,
    THERMAL = 1
  }

  internal enum NvmlSamplingType {
    TOTAL_POWER_SAMPLES = 0,        // To represent total power drawn by GPU
    GPU_UTILIZATION_SAMPLES = 1,    // To represent percent of time during which one or more kernels was executing on the GPU
    MEMORY_UTILIZATION_SAMPLES = 2, // To represent percent of time during which global (device) memory was being read or written
    ENC_UTILIZATION_SAMPLES = 3,    // To represent percent of time during which NVENC remains busy
    DEC_UTILIZATION_SAMPLES = 4,    // To represent percent of time during which NVDEC remains busy
    PROCESSOR_CLK_SAMPLES = 5,      // To represent processor clock samples
    MEMORY_CLK_SAMPLES = 6          // To represent memory clock samples
  }

  internal enum NvmlValueType {
    DOUBLE = 0,
    UNSIGNED_INT = 1,
    UNSIGNED_LONG = 2,
    UNSIGNED_LONG_LONG = 3
  }

  internal enum NvmlClocksThrottleReason : ulong {
    APPLICATIONS_CLOCKS_SETTING = 0x0000000000000002,
    GPU_IDLE = 0x0000000000000001,
    HW_SLOWDOWN = 0x0000000000000008,
    NONE = 0x0000000000000000,
    SW_POWER_CAP = 0x0000000000000004,
    UNKNOWN = 0x8000000000000000
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlDevice {
    private readonly IntPtr ptr;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlBAR1Memory {
    public ulong free;  // Unallocated BAR1 Memory(in bytes)
    public ulong total; // Total BAR1 Memory(in bytes)
    public ulong used;  // Allocated Used Memory(in bytes)
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlBridgeChipInfo {
    public uint fwVersion; // Firmware Version. 0=Version is unavailable
    public NvmlBridgeChipType type; // Type of Bridge Chip
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlBridgeChipHierarchy {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = NVML.MAX_PHYSICAL_BRIDGE)]
    public NvmlBridgeChipInfo[] bridgeChipInfo; // Hierarchy of Bridge Chips on the board
    public byte bridgeCount; // Number of Bridge Chips on the Board
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlMemory {
    public ulong total;
    public ulong free;
    public ulong used;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlPciInfo {
    public uint bus;            // The bus on which the device resides, 0 to 0xff
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVML.DEVICE_PCI_BUS_ID_BUFFER_SIZE)]
    public string busId;        // The tuple domain:bus:device.function PCI identifier(& NULL terminator)
    public uint device;         // The device's id on the bus, 0 to 31
    public uint domain;         // The PCI domain on which the device's bus resides, 0 to 0xffff
    public uint pciDeviceId;    // The combined 16-bit device id and 16-bit vendor id
    public uint pciSubSystemId; // The 32-bit Sub System Device ID
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlProcessInfo {
    public uint pid;
    public ulong usedGpuMemory;
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct NvmlValue {
    [FieldOffset(0)] public double dVal;
    [FieldOffset(0)] public ushort uiVal;
    [FieldOffset(0)] public uint ulVal;
    [FieldOffset(0)] public ulong ullVal;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlSample {
    public NvmlValue sampleValue;
    public ulong timeStamp; // CPU Timestamp in microseconds.
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlUtilization {
    public uint gpu;    // Percent of time over the past sample period during
                        //  which one or more kernels was executing on the GPU.
    public uint memory; // Percent of time over the past sample period during
                        //  which global (device) memory was being read or written.
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NvmlViolationTime {
    public ulong referenceTime;
    public ulong violationTime;
  }

  internal class NVML {
    public const string dllName = "nvml";
    public const string dllWinPath = @"%ProgramW6432%\NVIDIA Corporation\NVSMI\";
    public const int MAX_PATH = 260;

    public const int DEVICE_PCI_BUS_ID_BUFFER_SIZE = 16; // Buffer size guaranteed to be large enough for pci bus id
    public const int MAX_PHYSICAL_BRIDGE = 128; // Maximum limit on Physical Bridges per Board
    public const int VALUE_NOT_AVAILABLE = -1;  // Special constant that some fields take when they are not available

    // Buffer size guaranteed to be large enough 
    public const int DEVICE_INFOROM_VERSION_BUFFER_SIZE = 16; // for nvmlDeviceGetInforomVersion and nvmlDeviceGetInforomImageVersion
    public const int DEVICE_NAME_BUFFER_SIZE = 64;            // for nvmlDeviceGetName
    public const int DEVICE_PART_NUMBER_BUFFER_SIZE = 80;     // for nvmlDeviceGetBoardPartNumber
    public const int DEVICE_SERIAL_BUFFER_SIZE = 30;          // for nvmlDeviceGetSerial
    public const int DEVICE_UUID_BUFFER_SIZE = 80;            // for nvmlDeviceGetUUID
    public const int DEVICE_VBIOS_VERSION_BUFFER_SIZE = 32;   // for nvmlDeviceGetVbiosVersion
    public const int SYSTEM_DRIVER_VERSION_BUFFER_SIZE = 80;  // for nvmlSystemGetDriverVersion
    public const int SYSTEM_NVML_VERSION_BUFFER_SIZE = 80;    // for nvmlSystemGetNVMLVersion

    private bool available = false;
    private static readonly NVML instance = null;

    [DllImport(dllName, EntryPoint = "nvmlInit")]
    public static extern NvmlReturn NvmlInit();

    [DllImport(dllName, EntryPoint = "nvmlShutdown")]
    public static extern NvmlReturn NvmlShutdown();

    [DllImport(dllName, EntryPoint = "nvmlSystemGetDriverVersion")]
    public static extern NvmlReturn NvmlSystemGetDriverVersion(
      StringBuilder version, uint length);

    [DllImport(dllName, EntryPoint = "nvmlSystemGetNVMLVersion")]
    public static extern NvmlReturn NvmlSystemGetNVMLVersion(
      StringBuilder version, uint length);

    [DllImport(dllName, EntryPoint = "nvmlSystemGetProcessName")]
    public static extern NvmlReturn NvmlSystemGetProcessName(
      uint pid, StringBuilder name, uint length);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetAPIRestriction")]
    public static extern NvmlReturn NvmlDeviceGetAPIRestriction(
      NvmlDevice device, NvmlRestrictedAPI apiType, out NvmlEnableState isRestricted);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetApplicationsClock")]
    public static extern NvmlReturn NvmlDeviceGetApplicationsClock(
      NvmlDevice device, NvmlClockType clockType, out uint clockMHz);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetAutoBoostedClocksEnabled")]
    public static extern NvmlReturn NvmlDeviceGetAutoBoostedClocksEnabled(
      NvmlDevice device, out NvmlEnableState isEnabled, out NvmlEnableState defaultIsEnabled);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetBAR1MemoryInfo")]
    public static extern NvmlReturn NvmlDeviceGetBAR1MemoryInfo(
      NvmlDevice device, ref NvmlBAR1Memory bar1Memory);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetBoardId")]
    public static extern NvmlReturn NvmlDeviceGetBoardId(
      NvmlDevice device, out uint boardId);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetBoardPartNumber")]
    public static extern NvmlReturn NvmlDeviceGetBoardPartNumber(
      NvmlDevice device, StringBuilder partNumber, uint length);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetBrand")]
    public static extern NvmlReturn NvmlDeviceGetBrand(
      NvmlDevice device, out NvmlBrandType type);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetBridgeChipInfo")]
    public static extern NvmlReturn NvmlDeviceGetBridgeChipInfo(
      NvmlDevice device, ref NvmlBridgeChipHierarchy bridgeHierarchy);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetClockInfo")]
    public static extern NvmlReturn NvmlDeviceGetClockInfo(
      NvmlDevice device, NvmlClockType type, out uint clock);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetComputeMode")]
    public static extern NvmlReturn NvmlDeviceGetComputeMode(
      NvmlDevice device, out NvmlComputeMode mode);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetComputeRunningProcesses")]
    public static extern NvmlReturn NvmlDeviceGetComputeRunningProcesses(
      NvmlDevice device, ref uint infoCount, [Out] NvmlProcessInfo[] infos);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetCount")]
    public static extern NvmlReturn NvmlDeviceGetCount(
      out uint deviceCount);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetCpuAffinity")]
    public static extern NvmlReturn NvmlDeviceGetCpuAffinity(
      NvmlDevice device, uint cpuSetSize, out ulong cpuSet);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetCurrPcieLinkGeneration")]
    public static extern NvmlReturn NvmlDeviceGetCurrPcieLinkGeneration(
      NvmlDevice device, out uint currLinkGen);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetCurrPcieLinkWidth")]
    public static extern NvmlReturn NvmlDeviceGetCurrPcieLinkWidth(
      NvmlDevice device, out uint currLinkWidth);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetCurrentClocksThrottleReasons")]
    public static extern NvmlReturn NvmlDeviceGetCurrentClocksThrottleReasons(
      NvmlDevice device, out ulong clocksThrottleReasons);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetDecoderUtilization")]
    public static extern NvmlReturn NvmlDeviceGetDecoderUtilization(
      NvmlDevice device, out uint utilization, out uint samplingPeriodUs);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetDefaultApplicationsClock")]
    public static extern NvmlReturn NvmlDeviceGetDefaultApplicationsClock(
      NvmlDevice device, NvmlClockType clockType, out uint clockMHz);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetDisplayActive")]
    public static extern NvmlReturn NvmlDeviceGetDisplayActive(
      NvmlDevice device, out NvmlEnableState isActive);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetDisplayMode")]
    public static extern NvmlReturn NvmlDeviceGetDisplayMode(
      NvmlDevice device, out NvmlEnableState display);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetDriverModel")]
    public static extern NvmlReturn NvmlDeviceGetDriverModel(
      NvmlDevice device, out NvmlDriverModel current, out NvmlDriverModel pending);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetEccMode")]
    public static extern NvmlReturn NvmlDeviceGetEccMode(
      NvmlDevice device, out NvmlEnableState current, out NvmlEnableState pending);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetEncoderUtilization")]
    public static extern NvmlReturn NvmlDeviceGetEncoderUtilization(
      NvmlDevice device, out uint utilization, out uint samplingPeriodUs);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetEnforcedPowerLimit")]
    public static extern NvmlReturn NvmlDeviceGetEnforcedPowerLimit(
      NvmlDevice device, out uint limit);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetFanSpeed")]
    public static extern NvmlReturn NvmlDeviceGetFanSpeed(
      NvmlDevice device, out uint speed);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetGpuOperationMode")]
    public static extern NvmlReturn NvmlDeviceGetGpuOperationMode(
      NvmlDevice device, out NvmlGpuOperationMode current, out NvmlGpuOperationMode pending);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetGraphicsRunningProcesses")]
    public static extern NvmlReturn NvmlDeviceGetGraphicsRunningProcesses(
      NvmlDevice device, out uint infoCount, [Out] NvmlProcessInfo[] infos);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetHandleByIndex")]
    public static extern NvmlReturn NvmlDeviceGetHandleByIndex(
      uint index, ref NvmlDevice device);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetHandleByPciBusId")]
    public static extern NvmlReturn NvmlDeviceGetHandleByPciBusId(
      string pciBusId, ref NvmlDevice device);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetHandleByUUID")]
    public static extern NvmlReturn NvmlDeviceGetHandleByUUID(
      string uuid, ref NvmlDevice device);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetIndex")]
    public static extern NvmlReturn NvmlDeviceGetIndex(
      NvmlDevice device, out uint index);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetInforomConfigurationChecksum")]
    public static extern NvmlReturn NvmlDeviceGetInforomConfigurationChecksum(
      NvmlDevice device, out uint checksum);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetInforomImageVersion")]
    public static extern NvmlReturn NvmlDeviceGetInforomImageVersion(
      NvmlDevice device, StringBuilder version, uint length);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetInforomVersion")]
    public static extern NvmlReturn NvmlDeviceGetInforomVersion(
      NvmlDevice device, NvmlInforomObject obj, StringBuilder version, uint length);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetMaxClockInfo")]
    public static extern NvmlReturn NvmlDeviceGetMaxClockInfo(
      NvmlDevice device, NvmlClockType type, out uint clock);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetMaxPcieLinkGeneration")]
    public static extern NvmlReturn NvmlDeviceGetMaxPcieLinkGeneration(
      NvmlDevice device, out uint maxLinkGen);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetMaxPcieLinkWidth")]
    public static extern NvmlReturn NvmlDeviceGetMaxPcieLinkWidth(
      NvmlDevice device, out uint maxLinkWidth);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetMemoryErrorCounter")]
    public static extern NvmlReturn NvmlDeviceGetMemoryErrorCounter(
      NvmlDevice device, NvmlMemoryErrorType errorType, NvmlEccCounterType counterType,
      NvmlMemoryLocation locationType, out ulong count);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetMemoryInfo")]
    public static extern NvmlReturn NvmlDeviceGetMemoryInfo(
      NvmlDevice device, ref NvmlMemory memory);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetMinorNumber")]
    public static extern NvmlReturn NvmlDeviceGetMinorNumber(
      NvmlDevice device, out uint minorNumber);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetMultiGpuBoard")]
    public static extern NvmlReturn NvmlDeviceGetMultiGpuBoard(
      NvmlDevice device, out bool multiGpuBool);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetName")]
    public static extern NvmlReturn NvmlDeviceGetName(
      NvmlDevice device, StringBuilder name, uint length);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPciInfo")]
    public static extern NvmlReturn NvmlDeviceGetPciInfo(
      NvmlDevice device, ref NvmlPciInfo pci);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPcieReplayCounter")]
    public static extern NvmlReturn NvmlDeviceGetPcieReplayCounter(
      NvmlDevice device, out uint value);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPcieThroughput")]
    public static extern NvmlReturn NvmlDeviceGetPcieThroughput(
      NvmlDevice device, NvmlPcieUtilCounter counter, out uint value);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPerformanceState")]
    public static extern NvmlReturn NvmlDeviceGetPerformanceState(
      NvmlDevice device, out NvmlPstates pState);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPersistenceMode")]
    public static extern NvmlReturn NvmlDeviceGetPersistenceMode(
      NvmlDevice device, out NvmlEnableState mode);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPowerManagementDefaultLimit")]
    public static extern NvmlReturn NvmlDeviceGetPowerManagementDefaultLimit(
      NvmlDevice device, out uint defaultLimit);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPowerManagementLimit")]
    public static extern NvmlReturn NvmlDeviceGetPowerManagementLimit(
      NvmlDevice device, out uint limit);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPowerManagementLimitConstraints")]
    public static extern NvmlReturn NvmlDeviceGetPowerManagementLimitConstraints(
      NvmlDevice device, out uint minLimit, out uint maxLimit);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPowerManagementMode")]
    public static extern NvmlReturn NvmlDeviceGetPowerManagementMode(
      NvmlDevice device, out NvmlEnableState mode);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPowerState")]
    public static extern NvmlReturn NvmlDeviceGetPowerState(
      NvmlDevice device, out NvmlPstates pState);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetPowerUsage")]
    public static extern NvmlReturn NvmlDeviceGetPowerUsage(
      NvmlDevice device, out uint power);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetRetiredPages")]
    public static extern NvmlReturn NvmlDeviceGetRetiredPages(
      NvmlDevice device, NvmlPageRetirementCause cause, out uint pageCount, out ulong addresses);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetRetiredPagesPendingStatus")]
    public static extern NvmlReturn NvmlDeviceGetRetiredPagesPendingStatus(
      NvmlDevice device, out NvmlEnableState isPending);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetSamples")]
    public static extern NvmlReturn NvmlDeviceGetSamples(
      NvmlDevice device, NvmlSamplingType type, ulong lastSeenTimeStamp,
      out NvmlValueType sampleValType, out uint sampleCount, [Out] NvmlSample[] samples);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetSerial")]
    public static extern NvmlReturn NvmlDeviceGetSerial(
      NvmlDevice device, StringBuilder serial, uint length);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetSupportedClocksThrottleReasons")]
    public static extern NvmlReturn NvmlDeviceGetSupportedClocksThrottleReasons(
      NvmlDevice device, out ulong supportedClocksThrottleReasons);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetSupportedGraphicsClocks")]
    public static extern NvmlReturn NvmlDeviceGetSupportedGraphicsClocks(
      NvmlDevice device, uint memoryClockMHz, out uint count, [Out] uint[] clocksMHz);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetSupportedMemoryClocks")]
    public static extern NvmlReturn NvmlDeviceGetSupportedMemoryClocks(
      NvmlDevice device, out uint count, [Out] uint[] clocksMHz);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetTemperature")]
    public static extern NvmlReturn NvmlDeviceGetTemperature(
      NvmlDevice device, NvmlTemperatureSensors sensorType, out uint temp);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetTemperatureThreshold")]
    public static extern NvmlReturn NvmlDeviceGetTemperatureThreshold(
      NvmlDevice device, NvmlTemperatureThresholds thresholdType, out uint temp);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetTopologyCommonAncestor")]
    public static extern NvmlReturn NvmlDeviceGetTopologyCommonAncestor(
      NvmlDevice device1, NvmlDevice device2, out NvmlGpuTopologyLevel pathInfo);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetTopologyNearestGpus")]
    public static extern NvmlReturn NvmlDeviceGetTopologyNearestGpus(
      NvmlDevice device, NvmlGpuTopologyLevel level, out uint count, ref NvmlDevice deviceArray);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetTotalEccErrors")]
    public static extern NvmlReturn NvmlDeviceGetTotalEccErrors(
      NvmlDevice device, NvmlMemoryErrorType errorType, NvmlEccCounterType counterType, out ulong eccCounts);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetUUID")]
    public static extern NvmlReturn NvmlDeviceGetUUID(
      NvmlDevice device, StringBuilder uuid, uint length);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetUtilizationRates")]
    public static extern NvmlReturn NvmlDeviceGetUtilizationRates(
      NvmlDevice device, ref NvmlUtilization utilization);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetVbiosVersion")]
    public static extern NvmlReturn NvmlDeviceGetVbiosVersion(
      NvmlDevice device, StringBuilder version, uint length);

    [DllImport(dllName, EntryPoint = "nvmlDeviceGetViolationStatus")]
    public static extern NvmlReturn NvmlDeviceGetViolationStatus(
      NvmlDevice device, NvmlPerfPolicyType perfPolicyType, ref NvmlViolationTime violTime);

    [DllImport(dllName, EntryPoint = "nvmlDeviceOnSameBoard")]
    public static extern NvmlReturn NvmlDeviceOnSameBoard(
      NvmlDevice device1, NvmlDevice device2, out bool onSameBoard);

    [DllImport(dllName, EntryPoint = "nvmlDeviceValidateInforom")]
    public static extern NvmlReturn NvmlDeviceValidateInforom(
      NvmlDevice device);

    [DllImport(dllName, EntryPoint = "nvmlSystemGetTopologyGpuSet")]
    public static extern NvmlReturn NvmlSystemGetTopologyGpuSet(
      uint cpuNumber, out uint count, ref NvmlDevice deviceArray);

    private NVML() {
      if (IntPtr.Size == 4) // NVML is not supported on x86
        return;

      string cwd = Directory.GetCurrentDirectory();
      int p = (int)Environment.OSVersion.Platform;
      if ((p != 4) && (p != 128)) {
        string path = Environment.ExpandEnvironmentVariables(dllWinPath);

        try {
          Directory.SetCurrentDirectory(path);
        } catch {
          return;
        }
      }

      if (NvmlInit() != NvmlReturn.SUCCESS)
        return;

      if ((p != 4) && (p != 128))
        Directory.SetCurrentDirectory(cwd);

      available = true;
    }

    public static void Close() {
      if (instance.available) {
        NvmlShutdown();
        instance.available = false;
      }
    }

    static NVML() {
      if (instance == null)
        instance = new NVML();
    }

    public static bool IsAvailable {
      get { return instance.available; }
    }

    public static NvmlReturn GetDeviceName(uint index, out string name) {
      NvmlReturn result;
      NvmlDevice device = new NvmlDevice();

      result = NvmlDeviceGetHandleByIndex(index, ref device);
      if (result == NvmlReturn.SUCCESS)
        return GetDeviceName(device, out name);
      else
        name = "";

      return result;
    }

    public static NvmlReturn GetDeviceName(NvmlDevice device, out string name) {
      StringBuilder builder = new StringBuilder(DEVICE_NAME_BUFFER_SIZE);
      NvmlReturn result = NvmlDeviceGetName(device, builder, DEVICE_NAME_BUFFER_SIZE);
      name = builder.ToString();
      return result;
    }

    public static NvmlReturn GetDeviceUUID(uint index, out string uuid) {
      NvmlReturn result;
      NvmlDevice device = new NvmlDevice();

      result = NvmlDeviceGetHandleByIndex(index, ref device);
      if (result == NvmlReturn.SUCCESS)
        return GetDeviceUUID(device, out uuid);
      else
        uuid = "";

      return result;
    }

    public static NvmlReturn GetDeviceUUID(NvmlDevice device, out string uuid) {
      StringBuilder builder = new StringBuilder(DEVICE_UUID_BUFFER_SIZE);
      NvmlReturn result = NvmlDeviceGetUUID(device, builder, DEVICE_UUID_BUFFER_SIZE);
      uuid = builder.ToString();
      return result;
    }

    public static NvmlReturn GetDeviceVbiosVersionString(uint index, out string version) {
      NvmlReturn result;
      NvmlDevice device = new NvmlDevice();

      result = NvmlDeviceGetHandleByIndex(index, ref device);
      if (result == NvmlReturn.SUCCESS)
        return GetDeviceVbiosVersionString(device, out version);
      else
        version = "";

      return result;
    }

    public static NvmlReturn GetDeviceVbiosVersionString(NvmlDevice device, out string version) {
      StringBuilder builder = new StringBuilder(DEVICE_VBIOS_VERSION_BUFFER_SIZE);
      NvmlReturn result = NvmlDeviceGetVbiosVersion(device, builder, DEVICE_VBIOS_VERSION_BUFFER_SIZE);
      version = builder.ToString();
      return result;
    }

    public static NvmlReturn GetNvmlVersionString(out string version) {
      StringBuilder builder = new StringBuilder(SYSTEM_NVML_VERSION_BUFFER_SIZE);
      NvmlReturn result = NvmlSystemGetNVMLVersion(builder, SYSTEM_NVML_VERSION_BUFFER_SIZE);
      version = builder.ToString();
      return result;
    }

    public static NvmlReturn GetDriverVersionString(out string version) {
      StringBuilder builder = new StringBuilder(SYSTEM_DRIVER_VERSION_BUFFER_SIZE);
      NvmlReturn result = NvmlSystemGetDriverVersion(builder, SYSTEM_DRIVER_VERSION_BUFFER_SIZE);
      version = builder.ToString();
      return result;
    }
  }
}