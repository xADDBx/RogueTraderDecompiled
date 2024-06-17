using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("097b5f538f844b69a517ba24758d78c7")]
public class LevelUpFxLibrary : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<LevelUpFxLibrary>
	{
	}

	[SerializeField]
	public PrefabLink LevelUpFx;

	[AkEventReference]
	public string SoundFx;
}
