using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kingmaker.Sound;

[CreateAssetMenu(menuName = "AudioFilePackagesSettings")]
public class AudioFilePackagesSettings : ScriptableObject
{
	public enum AudioChunk
	{
		MainGame,
		DLC1
	}

	[Serializable]
	public class Mapping
	{
		[SerializeField]
		public List<string> Values;
	}

	private const string PathPrefix = "Packages";

	private static AudioFilePackagesSettings s_Instance;

	[SerializeField]
	private List<Mapping> m_PackageMapping;

	[SerializeField]
	private List<Mapping> m_BankMapping;

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

	public void LoadPackagesChunk(AudioChunk chunk)
	{
		EnsureInitialized();
		if (m_PackageMapping == null || m_PackageMapping.Count <= (int)chunk || chunk < AudioChunk.MainGame)
		{
			return;
		}
		foreach (string value in m_PackageMapping[(int)chunk].Values)
		{
			SoundPackagesManager.LoadPackage(value);
		}
	}

	public void UnloadPackagesChunk(AudioChunk chunk)
	{
		if (m_PackageMapping == null || m_PackageMapping.Count <= (int)chunk || chunk < AudioChunk.MainGame)
		{
			return;
		}
		foreach (string value in m_PackageMapping[(int)chunk].Values)
		{
			SoundPackagesManager.UnloadPackage(value);
		}
	}

	public void LoadBanksChunk(AudioChunk chunk)
	{
		if (m_BankMapping == null || m_BankMapping.Count <= (int)chunk || chunk < AudioChunk.MainGame)
		{
			return;
		}
		foreach (string value in m_BankMapping[(int)chunk].Values)
		{
			SoundBanksManager.LoadBankSync(value);
		}
	}

	public void UnloadBanksChunk(AudioChunk chunk)
	{
		if (m_BankMapping != null && m_BankMapping.Count > (int)chunk && chunk >= AudioChunk.MainGame)
		{
			SoundBanksManager.MarkBanksToUnload(m_BankMapping[(int)chunk].Values);
		}
	}

	private void EnsureInitialized()
	{
		AkSoundEngine.AddBasePath(Path.Combine(AkBasePathGetter.GetPlatformBasePath(), "Packages"));
	}
}
