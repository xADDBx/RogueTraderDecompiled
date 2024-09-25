using System;
using Kingmaker.Blueprints;

namespace Kingmaker.AreaLogic.QuestSystem;

public class UnlockableFlagsManagerWrapper
{
	private static UnlockableFlagsManagerWrapper s_Instance;

	private UnlockableFlagsManager m_FlagsManager;

	public static UnlockableFlagsManagerWrapper Instance => s_Instance ?? (s_Instance = new UnlockableFlagsManagerWrapper());

	private bool m_Initialized => m_FlagsManager != null;

	private void EnsureInitialized()
	{
		if (!m_Initialized)
		{
			throw new Exception("UnlockableFlagsManagerWrapper is not initialized!");
		}
	}

	public int GetFlagValue(BlueprintUnlockableFlag flag)
	{
		EnsureInitialized();
		return m_FlagsManager.GetFlagValue(flag);
	}

	public void Setup(UnlockableFlagsManager unlockableFlagsManager)
	{
		m_FlagsManager = unlockableFlagsManager;
	}

	public void SetFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		EnsureInitialized();
		m_FlagsManager.SetFlagValue(flag, value);
	}

	public bool IsUnlocked(BlueprintUnlockableFlag flag)
	{
		EnsureInitialized();
		return m_FlagsManager.IsUnlocked(flag);
	}

	public bool IsLocked(BlueprintUnlockableFlag flag)
	{
		EnsureInitialized();
		return m_FlagsManager.IsLocked(flag);
	}

	public void Lock(BlueprintUnlockableFlag flag)
	{
		EnsureInitialized();
		m_FlagsManager.Lock(flag);
	}

	public void Unlock(BlueprintUnlockableFlag flag)
	{
		EnsureInitialized();
		m_FlagsManager.Unlock(flag);
	}
}
