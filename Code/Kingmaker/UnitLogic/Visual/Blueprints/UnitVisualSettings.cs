using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.ResourceLinks;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Visual.Blueprints;

[Serializable]
public class UnitVisualSettings
{
	public enum MusicCombatState
	{
		Normal,
		Hard
	}

	public static readonly UnitVisualSettings Empty = new UnitVisualSettings();

	[SerializeField]
	public BloodType BloodType;

	public SurfaceType SurfaceType;

	public ShieldType ShieldType;

	public PrefabLink[] FootprintsOverride = new PrefabLink[0];

	public PrefabLink ArmorFx = new PrefabLink();

	public PrefabLink BloodPuddleFx = new PrefabLink();

	public PrefabLink DismemberFx = new PrefabLink();

	public PrefabLink RipLimbsApartFx = new PrefabLink();

	public bool IsNotUseDismember;

	[SerializeField]
	[FormerlySerializedAs("Barks")]
	private BlueprintUnitAsksListReference m_Barks;

	public AkSwitchReference FootTypeSoundSwitch = new AkSwitchReference();

	public AkSwitchReference FootSizeSoundSwitch = new AkSwitchReference();

	public AkSwitchReference BodyTypeSoundSwitch = new AkSwitchReference();

	public AkSwitchReference BodySizeSoundSwitch = new AkSwitchReference();

	[Header("Turret rotate sound")]
	[Tooltip("Set it only for turrets")]
	[AkEventReference]
	public string TurettRotateStart;

	[Tooltip("Set it only for turrets")]
	[AkEventReference]
	public string TurettRotateStop;

	[Space(10f)]
	public string FoleySoundPrefix;

	public bool NoFinishingBlow;

	public int ImportanceOverride;

	public bool SilentCaster;

	public MusicCombatState CombatMusic;

	public BlueprintUnitAsksList Barks => m_Barks?.Get();

	public GameObject GetBloodPuddle(SurfaceType bloodType)
	{
		if (BloodPuddleFx.Exists())
		{
			return BloodPuddleFx.Load();
		}
		return BlueprintRoot.Instance.HitSystemRoot.GetBloodPuddle(bloodType);
	}

	public GameObject GetDismember(SurfaceType surfaceType, UnitDismemberType dismemberType)
	{
		if (dismemberType == UnitDismemberType.Normal && DismemberFx.Exists())
		{
			return DismemberFx.Load();
		}
		if (dismemberType == UnitDismemberType.LimbsApart && RipLimbsApartFx.Exists())
		{
			return RipLimbsApartFx.Load();
		}
		return BlueprintRoot.Instance.HitSystemRoot.GetDismember(surfaceType, dismemberType);
	}
}
