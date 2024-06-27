using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Core.Reflection;
using Kingmaker.GameInfo;
using Kingmaker.Utility.ModsInfo;

namespace Kingmaker.Utility.Reporting.Base;

public static class ReportDllChecksumManager
{
	private static List<string> UmmDynamicDlls = new List<string> { "dnlib", "Ionic.Zip", "UnityModManager" };

	public static bool IsUnityModManagerActive()
	{
		return UserModsData.Instance.PlayingWithMods;
	}

	public static CheckSumInfo[] GetDllCRC()
	{
		try
		{
			return (from v in AppDomain.CurrentDomain.GetAssembliesSafe().AsParallel()
				where !v.IsDynamic && !UmmDynamicDlls.Contains(v.GetName().Name)
				select new CheckSumInfo
				{
					AssemblyName = v.FullName,
					Path = v.Location,
					Sha1 = CalcSha1(v.Location)
				}).ToArray();
		}
		catch (Exception)
		{
			return Array.Empty<CheckSumInfo>();
		}
	}

	public static string CalcSha1(string filePath)
	{
		try
		{
			using SHA1 sHA = SHA1.Create();
			using FileStream inputStream = File.OpenRead(filePath);
			return BitConverter.ToString(sHA.ComputeHash(inputStream));
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}
}
