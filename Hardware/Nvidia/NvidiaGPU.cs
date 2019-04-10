/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
	Copyright (C) 2019 Paweł Kania
 
*/

using System;
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.Nvidia {
  internal class NvidiaGPU : Hardware {
    private readonly NvmlDevice handle;

    private readonly Sensor[] clocks;
    private readonly Sensor thermal;
    private readonly Sensor fan;
    private readonly Sensor[] powers;
    private readonly Sensor[] loads;
    private readonly Sensor[] memory;

    private static readonly double byteConv = Math.Pow(1024, 2);

    public NvidiaGPU(uint deviceIndex, ISettings settings)
      : base(GetName(deviceIndex), new Identifier("nvidiagpu",
          deviceIndex.ToString(CultureInfo.InvariantCulture)), settings) {

      if (NVML.NvmlDeviceGetHandleByIndex(deviceIndex, ref handle) != NvmlReturn.SUCCESS)
        return;

      clocks = new Sensor[3];
      clocks[0] = new Sensor("GPU Core", 0, SensorType.Clock, this, settings);
      clocks[1] = new Sensor("GPU Memory", 2, SensorType.Clock, this, settings);
      clocks[2] = new Sensor("GPU Shader", 3, SensorType.Clock, this, settings);
      foreach (var clock in clocks)
        ActivateSensor(clock);

      thermal = new Sensor("GPU Core", 0, SensorType.Temperature, this, settings);
      ActivateSensor(thermal);

      // Shows % instead of RPM
      //fan = new Sensor("GPU Fan", 0, SensorType.Fan, this, settings);
      fan = new Sensor("GPU Fan", 0, SensorType.Level, this, settings);
      ActivateSensor(fan);

      powers = new Sensor[2];
      powers[0] = new Sensor("GPU Power", 0, SensorType.Power, this, settings);
      powers[1] = new Sensor("GPU TDP", 1, SensorType.Level, this, settings);
      foreach (var power in powers)
        ActivateSensor(power);

      loads = new Sensor[3];
      loads[0] = new Sensor("GPU Core", 0, SensorType.Load, this, settings);
      loads[1] = new Sensor("GPU Memory Controller", 1, SensorType.Load, this, settings);
      loads[2] = new Sensor("GPU Video Engine", 2, SensorType.Load, this, settings);
      foreach (var load in loads)
        ActivateSensor(load);

      memory = new Sensor[4];
      memory[0] = new Sensor("GPU Memory", 3, SensorType.Load, this, settings);
      memory[1] = new Sensor("GPU Memory Total", 0, SensorType.SmallData, this, settings);
      memory[2] = new Sensor("GPU Memory Used", 1, SensorType.SmallData, this, settings);
      memory[3] = new Sensor("GPU Memory Free", 2, SensorType.SmallData, this, settings);
      foreach (var mem in memory)
        ActivateSensor(mem);
    }

    private static string GetName(uint deviceIndex) {
      NVML.GetDeviceName(deviceIndex, out string name);
      return name;
    }

    public override HardwareType HardwareType {
      get { return HardwareType.GpuNvidia; }
    }

    public override void Update() {
      foreach (var clock in clocks) {
        NVML.NvmlDeviceGetClockInfo(handle, (NvmlClockType)clock.Index, out uint clockMhz);
        clock.Value = clockMhz;
      }

      NVML.NvmlDeviceGetTemperature(handle, NvmlTemperatureSensors.GPU, out uint temp);
      thermal.Value = temp;

      NVML.NvmlDeviceGetFanSpeed(handle, out uint fanSpeed);
      fan.Value = fanSpeed;

      NVML.NvmlDeviceGetPowerUsage(handle, out uint powerUsage);
      powers[0].Value = powerUsage / 1000.0f;

      NVML.NvmlDeviceGetPowerManagementDefaultLimit(handle, out uint tdp);
      powers[1].Value = powerUsage * 100.0f / tdp;

      NvmlUtilization util = new NvmlUtilization();
      NVML.NvmlDeviceGetUtilizationRates(handle, ref util);
      loads[0].Value = util.gpu;
      loads[1].Value = util.memory;

      NVML.NvmlDeviceGetDecoderUtilization(handle, out uint videoUtil, out uint vidSamPer);
      loads[2].Value = videoUtil;

      NvmlMemory memInfo = new NvmlMemory();
      NVML.NvmlDeviceGetMemoryInfo(handle, ref memInfo);
      memory[0].Value = 100.0f * memInfo.used / memInfo.total;
      memory[1].Value = (float)(memInfo.total / byteConv);
      memory[2].Value = (float)(memInfo.used / byteConv);
      memory[3].Value = (float)(memInfo.free / byteConv);
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("Nvidia GPU");
      r.AppendLine();

      r.AppendFormat("Name: {0}{1}", name, Environment.NewLine);
      NVML.NvmlDeviceGetIndex(handle, out uint index);
      r.AppendFormat("Index: {0}{1}", index, Environment.NewLine);

      if (NVML.GetDriverVersionString(out string drvVer) == NvmlReturn.SUCCESS)
        r.AppendLine("Driver Version: " + drvVer);
      r.AppendLine();

      NvmlPciInfo pciInfo = new NvmlPciInfo();
      if (NVML.NvmlDeviceGetPciInfo(handle, ref pciInfo) == NvmlReturn.SUCCESS) {
        r.AppendLine("PCIBus: " + pciInfo.busId);
        r.Append("PCIBusId: 0x");
        r.AppendLine(pciInfo.bus.ToString("X", CultureInfo.InvariantCulture));
        r.Append("PCISubSystemID: 0x");
        r.AppendLine(pciInfo.pciSubSystemId.ToString("X", CultureInfo.InvariantCulture));
        r.Append("DeviceID: 0x");
        r.AppendLine(pciInfo.pciDeviceId.ToString("X", CultureInfo.InvariantCulture));
        r.AppendLine();
      }

      if (NVML.NvmlDeviceGetCurrPcieLinkGeneration(handle, out uint linkGen) == NvmlReturn.SUCCESS)
        r.AppendLine("PCIE Generation: " + linkGen);

      if (NVML.NvmlDeviceGetCurrPcieLinkWidth(handle, out uint linkWidth) == NvmlReturn.SUCCESS)
        r.AppendLine("PCIE Width: x" + linkWidth);
      r.AppendLine();

      if (NVML.GetDeviceUUID(handle, out string uuid) == NvmlReturn.SUCCESS)
        r.AppendLine("Device UUID: " + uuid);

      if (NVML.GetDeviceVbiosVersionString(handle, out string vbios) == NvmlReturn.SUCCESS)
        r.AppendLine("VBIOS Version: " + vbios);
      r.AppendLine();

      r.AppendLine();
      return r.ToString();
    }

    public override void Close() {
      NVML.Close();
      base.Close();
    }
  }
}
