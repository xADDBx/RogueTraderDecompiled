using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[TypeId("06e9a18b1f15bcf41b3a0ce1a2a0dfdd")]
[HashRoot]
public class BlueprintUnlockableFlag : BlueprintScriptableObject
{
	public bool IsUnlocked => UnlockableFlagsManagerWrapper.Instance.IsUnlocked(this);

	public bool IsLocked => UnlockableFlagsManagerWrapper.Instance.IsLocked(this);

	public int Value
	{
		get
		{
			return UnlockableFlagsManagerWrapper.Instance.GetFlagValue(this);
		}
		set
		{
			UnlockableFlagsManagerWrapper.Instance.SetFlagValue(this, value);
		}
	}

	public void Lock()
	{
		UnlockableFlagsManagerWrapper.Instance.Lock(this);
	}

	public void Unlock()
	{
		UnlockableFlagsManagerWrapper.Instance.Unlock(this);
	}
}
