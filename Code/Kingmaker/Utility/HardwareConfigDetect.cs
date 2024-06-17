using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kingmaker.Utility;

public class HardwareConfigDetect
{
	public enum HardwareLevel
	{
		Low,
		Medium,
		High
	}

	private struct HardwareConfig
	{
		public ushort CPU;

		public ushort GPU;

		public HardwareConfig(ushort cpu, ushort gpu)
		{
			CPU = cpu;
			GPU = gpu;
		}
	}

	private static readonly ushort s_CPUSignature = 44474;

	private static readonly ushort s_GPUSignature = 64959;

	private static readonly int s_DefaultConfigIndex = 2;

	private static readonly HardwareConfig[] s_Configs = new HardwareConfig[2]
	{
		new HardwareConfig(2000, 600),
		new HardwareConfig(5000, 3000)
	};

	private static bool s_Detected = false;

	private static int s_ConfigIndex = s_DefaultConfigIndex;

	private static bool GPULookup(int vendorID, int deviceID, string name, BinaryReader reader, ushort defaultBenchmark, out ushort benchmark)
	{
		ushort num = (ushort)((uint)vendorID & 0xFFFFu);
		ushort num2 = (ushort)((uint)deviceID & 0xFFFFu);
		string text = name.Trim().ToLowerInvariant();
		benchmark = defaultBenchmark;
		if (reader.ReadUInt16() != s_GPUSignature)
		{
			Debug.Log("GPULookup invalid database");
			return false;
		}
		ushort num3 = reader.ReadUInt16();
		for (ushort num4 = 0; num4 < num3; num4++)
		{
			ushort num5 = reader.ReadUInt16();
			ushort num6 = reader.ReadUInt16();
			ushort num7 = reader.ReadUInt16();
			if (num5 == num && num6 == num2)
			{
				benchmark = num7;
				Debug.Log($"GPULookup found, GPU vendorID: {vendorID}, deviceID: {deviceID}, name: {name}, benchmark: {benchmark}");
				return true;
			}
		}
		ushort num8 = reader.ReadUInt16();
		for (ushort num9 = 0; num9 < num8; num9++)
		{
			string text2 = reader.ReadString();
			ushort num10 = reader.ReadUInt16();
			if (text.Contains(text2))
			{
				benchmark = num10;
				Debug.Log($"GPULookup found, GPU vendorID: {vendorID}, deviceID: {deviceID}, name: {name}, DB: {text2}. benchmark: {benchmark}");
				return true;
			}
		}
		Debug.Log($"GPULookup not found, GPU vendorID: {vendorID}, deviceID: {deviceID}, name: {name}");
		return false;
	}

	private static bool CPULookup(string name, BinaryReader reader, ushort defaultBenchmark, out ushort benchmark)
	{
		string text = name.Trim().ToLowerInvariant();
		benchmark = defaultBenchmark;
		if (reader.ReadUInt16() != s_CPUSignature)
		{
			Debug.Log("CPULookup invalid database");
			return false;
		}
		ushort num = reader.ReadUInt16();
		for (ushort num2 = 0; num2 < num; num2++)
		{
			string text2 = reader.ReadString();
			ushort num3 = reader.ReadUInt16();
			if (text.Contains(text2) || text2.Contains(text))
			{
				benchmark = num3;
				Debug.Log($"CPULookup found, CPU: {name}, DB: {text2}, benchmark: {benchmark}");
				return true;
			}
		}
		Debug.Log("CPULookup not found, CPU: " + name);
		return false;
	}

	public static HardwareLevel GetConfigIndex()
	{
		if (!s_Detected)
		{
			ushort num = 0;
			ushort num2 = 0;
			ushort benchmark = num;
			ushort benchmark2 = num2;
			bool flag = false;
			bool flag2 = false;
			Stream stream = null;
			BinaryReader binaryReader = null;
			try
			{
				stream = new MemoryStream((Resources.Load(Path.Combine("HardwareConfigs", "CPU")) as TextAsset).bytes);
				binaryReader = new BinaryReader(stream);
				flag = CPULookup(SystemInfo.processorType, binaryReader, num, out benchmark);
			}
			catch (Exception)
			{
			}
			finally
			{
				binaryReader?.Close();
				stream?.Close();
			}
			Stream stream2 = null;
			BinaryReader binaryReader2 = null;
			try
			{
				stream2 = new MemoryStream((Resources.Load(Path.Combine("HardwareConfigs", "GPU")) as TextAsset).bytes);
				binaryReader2 = new BinaryReader(stream2);
				flag2 = GPULookup(SystemInfo.graphicsDeviceVendorID, SystemInfo.graphicsDeviceID, SystemInfo.graphicsDeviceName, binaryReader2, num2, out benchmark2);
			}
			catch (Exception)
			{
			}
			finally
			{
				binaryReader2?.Close();
				stream2?.Close();
			}
			if (flag && flag2)
			{
				s_ConfigIndex = 0;
				while (s_ConfigIndex < 2 && benchmark >= s_Configs[s_ConfigIndex].CPU && benchmark2 >= s_Configs[s_ConfigIndex].GPU)
				{
					s_ConfigIndex++;
				}
			}
			else
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				if (!flag)
				{
					dictionary.Add("cpu_name", SystemInfo.processorType);
				}
				if (!flag2)
				{
					dictionary.Add("gpu_vendorid", SystemInfo.graphicsDeviceVendorID.ToString());
					dictionary.Add("gpu_deviceid", SystemInfo.graphicsDeviceID.ToString());
					dictionary.Add("gpu_name", SystemInfo.graphicsDeviceName);
				}
				AnalyticsHelper.SendEvent("report_unknown_hardware", dictionary);
				s_ConfigIndex = s_DefaultConfigIndex;
			}
			s_Detected = true;
		}
		HardwareLevel hardwareLevel = (HardwareLevel)s_ConfigIndex;
		float num3 = (float)SystemInfo.systemMemorySize / 1000f;
		switch (hardwareLevel)
		{
		case HardwareLevel.Medium:
			if (num3 <= 4f)
			{
				hardwareLevel = HardwareLevel.Low;
			}
			break;
		case HardwareLevel.High:
			if (num3 < 8f)
			{
				hardwareLevel = HardwareLevel.Medium;
			}
			if (num3 <= 4f)
			{
				hardwareLevel = HardwareLevel.Low;
			}
			break;
		}
		return hardwareLevel;
	}
}
