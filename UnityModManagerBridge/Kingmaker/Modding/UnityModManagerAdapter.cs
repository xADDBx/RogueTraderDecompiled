using System.Collections.Generic;
using Code.Utility.ExtendedModInfo;

namespace Kingmaker.Modding;

public class UnityModManagerAdapter
{
	private static UnityModManagerAdapter s_Instance;

	private readonly IModManagerBridge m_Bridge;

	public static UnityModManagerAdapter Instance => s_Instance ?? (s_Instance = new UnityModManagerAdapter());

	public bool ExternalUmmUsed
	{
		get
		{
			IModManagerBridge bridge = m_Bridge;
			if (bridge != null && bridge.DoorstopUsed.HasValue)
			{
				return m_Bridge.DoorstopUsed.Value;
			}
			return false;
		}
	}

	private UnityModManagerAdapter()
	{
		m_Bridge = new UnityModManagerBridge();
	}

	public void OpenModInfoWindow(string modId)
	{
		m_Bridge.OpenModInfoWindow(modId);
	}

	public void CheckForUpdates()
	{
		m_Bridge.CheckForUpdates();
	}

	public ExtendedModInfo GetModInfo(string modId)
	{
		return m_Bridge.GetModInfo(modId);
	}

	public List<ExtendedModInfo> GetAllModsInfo()
	{
		return m_Bridge.GetAllModsInfo();
	}

	public void EnableMod(string modId, bool state)
	{
		m_Bridge.EnableMod(modId, state);
	}

	public void TryStart(string passedGameVersion)
	{
		m_Bridge.TryStart(passedGameVersion);
	}

	public void TryStartUI()
	{
		m_Bridge.TryStartUI();
	}
}
