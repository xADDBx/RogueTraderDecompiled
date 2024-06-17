using System.Collections.Generic;
using Kingmaker._TmpTechArt;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

public static class DismembermentHandler
{
	public enum DeathType
	{
		Dismemberment,
		RagDoll,
		Animation
	}

	public static bool ShouldDismember(AbstractUnitEntity unit)
	{
		int num;
		if (!unit.Blueprint.VisualSettings.IsNotUseDismember && !unit.Features.SuppressedDismember && !(unit is BaseUnitEntity { IsMainCharacter: not false }))
		{
			UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
			if (optional == null || optional.State == CompanionState.None)
			{
				num = ((unit.OriginalSize >= Size.Huge) ? 1 : 0);
				goto IL_0057;
			}
		}
		num = 1;
		goto IL_0057;
		IL_0057:
		bool isFinallyDead = unit.LifeState.IsFinallyDead;
		if (num != 0 || !isFinallyDead)
		{
			return false;
		}
		if (unit.LifeState.ForceDismember == UnitDismemberType.ForcedNone)
		{
			return false;
		}
		if (unit.LifeState.ForceDismember != 0)
		{
			return true;
		}
		return GetDeathType(unit) == DeathType.Dismemberment;
	}

	public static UnitDismemberType GetDismemberType(AbstractUnitEntity unit)
	{
		UnitDismemberType forceDismember = unit.LifeState.ForceDismember;
		if (forceDismember != 0 && forceDismember != UnitDismemberType.ForcedNone)
		{
			return forceDismember;
		}
		bool flag = false;
		bool flag2 = false;
		BlueprintComponent[] array = unit?.Health.LastHandledDamage?.Reason.Context?.SourceAbility?.ComponentsArray;
		if (array != null)
		{
			if (array.Any((BlueprintComponent comp) => comp is InPowerDismemberComponent))
			{
				flag = true;
			}
			if (array.Any((BlueprintComponent comp) => comp is SplitDismemberComponent))
			{
				flag2 = true;
			}
		}
		bool flag3 = CanDismemberLimbsApart(unit);
		flag2 = flag2 && flag3;
		HitSystemRoot hitSystemRoot = BlueprintRoot.Instance.HitSystemRoot;
		if (flag && flag2)
		{
			float num = PFStatefulRandom.View.Range(0f, 100f);
			if (hitSystemRoot.LimbsApartDismembermentChance > 0f && num <= hitSystemRoot.LimbsApartDismembermentChance)
			{
				return UnitDismemberType.LimbsApart;
			}
			return UnitDismemberType.InPower;
		}
		if (flag)
		{
			return UnitDismemberType.InPower;
		}
		if (flag2)
		{
			return UnitDismemberType.LimbsApart;
		}
		if (flag3)
		{
			float num2 = PFStatefulRandom.View.Range(0f, 100f);
			if (hitSystemRoot.LimbsApartDismembermentChance > 0f && num2 <= hitSystemRoot.LimbsApartDismembermentChance)
			{
				return UnitDismemberType.LimbsApart;
			}
			return UnitDismemberType.Normal;
		}
		return UnitDismemberType.Normal;
	}

	private static bool CanDismemberLimbsApart(AbstractUnitEntity unit)
	{
		if (unit.View.LimbsApartDismembermentRestricted)
		{
			return false;
		}
		if (unit.View.DismembermentManager == null)
		{
			return false;
		}
		if (unit.View.RigidbodyController == null)
		{
			return false;
		}
		if (unit.View.DismembermentManager.LastImpulse == null || Game.Instance.TimeController.RealTime - unit.View.DismembermentManager.LastImpulse.Time > 0.2f.Seconds())
		{
			return false;
		}
		return unit.View.DismembermentManager.LastImpulse.DamageType != DamageType.Toxic;
	}

	public static DeathType GetDeathType(AbstractUnitEntity unit)
	{
		if (!unit.LifeState.IsFinallyDead)
		{
			return DeathType.Animation;
		}
		if (unit.LifeState.ForceDismember == UnitDismemberType.LimbsApart && unit?.View.DismembermentManager != null)
		{
			return DeathType.Dismemberment;
		}
		DeathType deathType;
		switch (unit.GetOptional<PartHealth>()?.LastHandledDamage?.Damage.Type)
		{
		case DamageType.Fire:
		case DamageType.Energy:
		case DamageType.Neural:
		case DamageType.Direct:
			deathType = DeathType.RagDoll;
			break;
		case DamageType.Impact:
		case DamageType.Rending:
		case DamageType.Piercing:
		case DamageType.Toxic:
		case DamageType.Warp:
			if (unit.GetOptional<PartHealth>()?.LastHandledDamage?.Result > (BlueprintWarhammerRoot.Instance.BlueprintDismembermentRoot?.MinimumDamageForDismember ?? 8))
			{
				deathType = DeathType.Dismemberment;
			}
			else
			{
				deathType = DeathType.RagDoll;
			}
			break;
		default:
			deathType = DeathType.Animation;
			break;
		}
		int num = 0;
		List<WeaponsListDism.PrefabInPair> obj = BlueprintWarhammerRoot.Instance.BlueprintDismembermentRoot?.WeaponsListDismArray.WeaponsChancesArray;
		string text = unit?.GetOptional<PartHealth>()?.LastHandledDamage?.Reason.Ability?.Weapon?.Blueprint?.VisualParameters?.Model?.name;
		foreach (WeaponsListDism.PrefabInPair item in obj)
		{
			if (text != null && item.PrefabPairGo != null && item.PrefabPairGo.name.ToLower().Equals(text.ToLower()))
			{
				num = item.PrefabPairInt;
			}
		}
		deathType = ((((unit.Random.Range(0, 100) < num) ? 1 : 0) != 1) ? DeathType.RagDoll : DeathType.Dismemberment);
		bool? obj2 = BlueprintWarhammerRoot.Instance.BlueprintDismembermentRoot?.WeaponsListDismArray.ForceRagdoll;
		bool? flag = BlueprintWarhammerRoot.Instance.BlueprintDismembermentRoot?.WeaponsListDismArray.ForceDismemberment;
		if (obj2 == true)
		{
			deathType = DeathType.RagDoll;
		}
		if (flag == true)
		{
			deathType = DeathType.Dismemberment;
		}
		if (!SettingsRoot.Game.Main.DismemberCharacters.GetValue())
		{
			deathType = DeathType.RagDoll;
		}
		if (deathType == DeathType.Dismemberment && unit.View.DismembermentManager == null)
		{
			deathType = DeathType.RagDoll;
		}
		if (deathType == DeathType.RagDoll && unit.View.RigidbodyController == null)
		{
			deathType = DeathType.Animation;
		}
		return deathType;
	}

	public static bool CanUseAnimation(AbstractUnitEntity unit)
	{
		return GetDeathType(unit) == DeathType.Animation;
	}

	public static void UseWithoutAnimationDeath(AbstractUnitEntity unit)
	{
		DeathType deathType = GetDeathType(unit);
		if (deathType == DeathType.Dismemberment)
		{
			unit?.View.DismembermentManager.StartDismemberment(unit.Random, unit.LifeState.DismembermentLimbsApartType);
		}
		if (deathType == DeathType.RagDoll)
		{
			unit?.View.RigidbodyController.StartRagdoll();
		}
		if (deathType != DeathType.RagDoll && deathType != 0)
		{
			return;
		}
		PrefabLink prefabLink = BlueprintWarhammerRoot.Instance.BlueprintDismembermentRoot?.FXDamagePairs?.FirstOrDefault((BlueprintDismembermentRoot.FXDamagePair i) => i.DamageType == unit?.GetOptional<PartHealth>()?.LastHandledDamage?.Damage.Type)?.FX;
		PrefabLink prefabLink2 = BlueprintWarhammerRoot.Instance.BlueprintDismembermentRoot?.PrefabDamagePairs?.FirstOrDefault((BlueprintDismembermentRoot.PrefabDamagePair i) => i.FxPrefab == unit?.GetOptional<PartHealth>()?.LastHandledDamage?.Reason.Ability?.FXSettings?.VisualFXSettings.Projectiles[0].View)?.FX2;
		if (prefabLink2 != null)
		{
			prefabLink = prefabLink2;
		}
		if (prefabLink != null)
		{
			GameObject prefab = prefabLink.Load();
			if (deathType == DeathType.RagDoll)
			{
				FxHelper.SpawnFxOnEntity(prefab, unit?.View);
			}
			if (deathType == DeathType.Dismemberment && unit?.View.DismembermentManager != null)
			{
				FxHelper.SpawnFxOnGameObject(prefab, unit?.View.gameObject);
			}
		}
		SpawnFxOnStart component = unit.View.GetComponent<SpawnFxOnStart>();
		if ((bool)component)
		{
			if (deathType == DeathType.Dismemberment)
			{
				component.HandleUnitDismemberment();
			}
			else
			{
				component.HandleUnitDeathRagdoll();
			}
		}
	}
}
