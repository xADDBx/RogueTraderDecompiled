using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

[JsonObject]
public class HardwareInfo
{
	[JsonProperty]
	public string CpuName;

	[JsonProperty]
	public string GpuName;

	[JsonProperty]
	public string GpuVendorid;

	[JsonProperty]
	public string GpuDeviceid;

	[JsonProperty]
	public string GpuMemSize;

	[JsonProperty]
	public string RamSize;

	public HardwareInfo()
	{
		CpuName = SystemInfo.processorType;
		GpuName = SystemInfo.graphicsDeviceName;
		GpuVendorid = SystemInfo.graphicsDeviceVendorID.ToString();
		GpuDeviceid = SystemInfo.graphicsDeviceID.ToString();
		RamSize = ((float)SystemInfo.systemMemorySize / 1000f).ToString("0.0");
		GpuMemSize = ((float)SystemInfo.graphicsMemorySize / 1000f).ToString("0.0");
	}
}
