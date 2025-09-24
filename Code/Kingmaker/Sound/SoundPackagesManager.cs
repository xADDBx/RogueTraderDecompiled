using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Sound;

public static class SoundPackagesManager
{
	public class PackageHandle
	{
		public string PackageName { get; }

		public bool Loaded { get; private set; }

		public int RefsCount { get; private set; }

		public PackageHandle(string packageName)
		{
			PackageName = packageName;
		}

		public void Load()
		{
			RefsCount++;
			if (!Loaded)
			{
				AkSoundEngine.LoadFilePackage(PackageName, out var _);
				Loaded = true;
			}
		}

		public void Unload()
		{
			RefsCount--;
			if (RefsCount <= 0)
			{
				if (RefsCount < 0)
				{
					RefsCount = 0;
					PFLog.Audio.Warning("Package was already unloaded");
				}
				else
				{
					Loaded = false;
					AkSoundEngine.UnloadFilePackage(AkSoundEngine.GetIDFromString(PackageName));
				}
			}
		}
	}

	private static readonly Dictionary<string, PackageHandle> s_Handles = new Dictionary<string, PackageHandle>();

	public static void LoadPackage(string packageName)
	{
		if (!AkAudioService.IsPackagesEnabled)
		{
			return;
		}
		if (string.IsNullOrEmpty(packageName))
		{
			PFLog.Audio.Error("Can't load Package with no name!");
			return;
		}
		PackageHandle packageHandle = s_Handles.Get(packageName);
		if (packageHandle == null)
		{
			packageHandle = new PackageHandle(packageName);
			s_Handles[packageName] = packageHandle;
		}
		packageHandle.Load();
	}

	public static void UnloadPackage(string packageName)
	{
		if (AkAudioService.IsPackagesEnabled)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				PFLog.Audio.Error("Can't unload Package with no name!");
			}
			else
			{
				s_Handles.Get(packageName)?.Unload();
			}
		}
	}
}
