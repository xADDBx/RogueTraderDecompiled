using System;
using System.Collections.Generic;
using Kingmaker.Stores;

namespace Kingmaker.Console.NintendoSwitch2;

public static class Switch2DlcFileSystem
{
	private readonly struct DlcMountPoint
	{
		public readonly byte[] FileSystemCache;

		public readonly string MountName;

		public DlcMountPoint(byte[] fileSystemCache, string mountName)
		{
			FileSystemCache = fileSystemCache;
			MountName = mountName;
		}
	}

	private static int[] s_AocTempBuffer;

	private static int s_AocCount = 0;

	private static readonly Dictionary<int, DlcMountPoint> DlcMountPoints = new Dictionary<int, DlcMountPoint>();

	public static void Update()
	{
		s_AocTempBuffer = Array.Empty<int>();
	}

	public static DLCStatus GetStatusById(int id)
	{
		Update();
		if (Array.IndexOf(s_AocTempBuffer, id, 0, s_AocCount) == -1)
		{
			return DLCStatus.UnAvailable;
		}
		return DLCStatus.Available;
	}

	public static void Mount(int idx)
	{
	}

	public static bool IsMounted(int idx)
	{
		return DlcMountPoints.ContainsKey(idx);
	}
}
