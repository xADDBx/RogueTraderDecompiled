using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[TypeId("37f91f51ea6c42179e3bff2c972cd6d6")]
public class BlueprintRomanceCounter : BlueprintScriptableObject
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("CounterFlag")]
	private BlueprintUnlockableFlagReference m_CounterFlag;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("MinValueFlag")]
	private BlueprintUnlockableFlagReference m_MinValueFlag;

	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("MaxValueFlag")]
	private BlueprintUnlockableFlagReference m_MaxValueFlag;

	public BlueprintUnlockableFlag CounterFlag => m_CounterFlag?.Get();

	public BlueprintUnlockableFlag MinValueFlag => m_MinValueFlag?.Get();

	public BlueprintUnlockableFlag MaxValueFlag => m_MaxValueFlag?.Get();

	public bool IsLocked
	{
		get
		{
			if (CounterFlag.Value >= 0)
			{
				return CounterFlag.IsLocked;
			}
			return true;
		}
	}

	public void Increase(int v = 1)
	{
		UnlockFlags();
		if (!IsLocked)
		{
			CounterFlag.Value = Mathf.Min(CounterFlag.Value + v, MaxValueFlag.Value);
		}
	}

	public void UnlockFlags()
	{
		CounterFlag.Unlock();
		MinValueFlag.Unlock();
		MaxValueFlag.Unlock();
	}

	public void Decrease(int v = 1)
	{
		UnlockFlags();
		if (!IsLocked)
		{
			CounterFlag.Value = Mathf.Max(CounterFlag.Value - v, MinValueFlag.Value);
		}
	}

	public void Lock()
	{
		UnlockFlags();
		CounterFlag.Value = -1;
		MinValueFlag.Value = -1;
		MaxValueFlag.Value = -1;
	}
}
