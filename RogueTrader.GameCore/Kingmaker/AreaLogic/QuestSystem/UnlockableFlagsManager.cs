using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

[HashRoot]
public class UnlockableFlagsManager : IHashable
{
	[JsonProperty]
	private readonly Dictionary<BlueprintUnlockableFlag, int> m_UnlockedFlags = new Dictionary<BlueprintUnlockableFlag, int>();

	public Dictionary<BlueprintUnlockableFlag, int> UnlockedFlags => m_UnlockedFlags;

	private IEnumerable<string> UnlocksInStateExplorer => m_UnlockedFlags.Select((KeyValuePair<BlueprintUnlockableFlag, int> p) => p.Key.name + ": " + p.Value);

	public void Lock(BlueprintUnlockableFlag flag)
	{
		m_UnlockedFlags.Remove(flag);
		EventBus.RaiseEvent(delegate(IUnlockHandler h)
		{
			h.HandleLock(flag);
		});
	}

	public void Unlock(BlueprintUnlockableFlag flag)
	{
		if (!IsUnlocked(flag))
		{
			m_UnlockedFlags.Add(flag, 0);
			EventBus.RaiseEvent(delegate(IUnlockHandler h)
			{
				h.HandleUnlock(flag);
			});
		}
	}

	public void SetFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		bool num = !m_UnlockedFlags.ContainsKey(flag);
		m_UnlockedFlags[flag] = value;
		if (num)
		{
			EventBus.RaiseEvent(delegate(IUnlockHandler h)
			{
				h.HandleUnlock(flag);
			});
		}
		EventBus.RaiseEvent(delegate(IUnlockValueHandler h)
		{
			h.HandleFlagValue(flag, value);
		});
	}

	public int GetFlagValue(BlueprintUnlockableFlag flag)
	{
		m_UnlockedFlags.TryGetValue(flag, out var value);
		return value;
	}

	public bool IsUnlocked(BlueprintUnlockableFlag flag)
	{
		return m_UnlockedFlags.ContainsKey(flag);
	}

	public bool IsLocked(BlueprintUnlockableFlag flag)
	{
		return !m_UnlockedFlags.ContainsKey(flag);
	}

	public override string ToString()
	{
		return "Unlocks";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Dictionary<BlueprintUnlockableFlag, int> unlockedFlags = m_UnlockedFlags;
		if (unlockedFlags != null)
		{
			int val = 0;
			foreach (KeyValuePair<BlueprintUnlockableFlag, int> item in unlockedFlags)
			{
				Hash128 hash = default(Hash128);
				Hash128 val2 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val2);
				int obj = item.Value;
				Hash128 val3 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val3);
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
		}
		return result;
	}
}
