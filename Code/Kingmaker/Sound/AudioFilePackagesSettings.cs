using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kingmaker.Sound;

[CreateAssetMenu(menuName = "AudioFilePackagesSettings")]
public class AudioFilePackagesSettings : ScriptableObject
{
	private const string PathPrefix = "Packages";

	private static AudioFilePackagesSettings s_Instance;

	[SerializeField]
	private List<string> m_PackageNames = new List<string>();

	public static AudioFilePackagesSettings Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = Resources.Load<AudioFilePackagesSettings>("AudioFilePackagesSettings");
			}
			if (s_Instance == null)
			{
				s_Instance = ScriptableObject.CreateInstance<AudioFilePackagesSettings>();
				PFLog.Audio.Warning("WwiseUnity: No platform specific settings were created. Default initialization settings will be used.");
			}
			return s_Instance;
		}
	}

	public void LoadPackages()
	{
		EnsureInitialized();
		foreach (string packageName in m_PackageNames)
		{
			SoundPackagesManager.LoadPackage(packageName);
		}
	}

	public void UnloadPackages()
	{
		foreach (string packageName in m_PackageNames)
		{
			SoundPackagesManager.UnloadPackage(packageName);
		}
	}

	private void EnsureInitialized()
	{
		AkSoundEngine.AddBasePath(Path.Combine(AkBasePathGetter.GetPlatformBasePath(), "Packages"));
	}
}
