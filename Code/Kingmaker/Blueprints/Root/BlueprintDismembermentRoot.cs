using System;
using Kingmaker._TmpTechArt;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.Blueprints.Root;

[TypeId("501753aa80cc4554a56e711956ea6ea1")]
public class BlueprintDismembermentRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintDismembermentRoot>
	{
	}

	[Serializable]
	public class FXDamagePair
	{
		public DamageType DamageType;

		public PrefabLink FX;
	}

	[Serializable]
	public class PrefabDamagePair
	{
		public ProjectileLink FxPrefab;

		public PrefabLink FX2;
	}

	public int MinimumDamageForDismember = 8;

	public FXDamagePair[] FXDamagePairs = new FXDamagePair[0];

	public PrefabDamagePair[] PrefabDamagePairs = new PrefabDamagePair[0];

	public WeaponsListDism WeaponsListDismArray;
}
