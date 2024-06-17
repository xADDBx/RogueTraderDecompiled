using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Area;

[Serializable]
public class UnlockValuePair
{
	[JsonProperty]
	[DependenciesFilter]
	[SerializeField]
	[FormerlySerializedAs("Flag")]
	private BlueprintUnlockableFlagReference m_Flag;

	[JsonProperty]
	public int Value;

	public BlueprintUnlockableFlag Flag
	{
		get
		{
			return m_Flag?.Get();
		}
		set
		{
			m_Flag = value.ToReference<BlueprintUnlockableFlagReference>();
		}
	}
}
