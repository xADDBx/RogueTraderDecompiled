using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

[TypeId("1fc31d2da33f5d84ab8f909e174fa7e2")]
public class HitSystemRoot : BlueprintScriptableObject
{
	[NotNull]
	public GlobalHitEffectEntry GlobalHitEffect = new GlobalHitEffectEntry();

	[NotNull]
	public DamageEntry[] DamageTypes = new DamageEntry[0];

	[NotNull]
	public HitEntry[] HitEffects = new HitEntry[0];

	[NotNull]
	public BloodEntry[] BloodTypes = new BloodEntry[0];

	[NotNull]
	public DamageHitSettings DefaultDamage = new DamageHitSettings();

	[NotNull]
	public DamageHitSettings DefaultAoeDamage = new DamageHitSettings();

	[NotNull]
	public BloodPrefabsFromWeaponAnimationStyleEntry[] OverrideHitDirectionPrefabFromAnimationStyle = new BloodPrefabsFromWeaponAnimationStyleEntry[0];

	public float MaxHeightIncrease;

	[CanBeNull]
	[AssetPicker("Assets/FX/Resources/Prefabs/")]
	public GameObject VoidShieldTurnOn;

	public GameObject VoidShieldTurnOnFront;

	public GameObject VoidShieldTurnOnBack;

	public GameObject VoidShieldTurnOnLeft;

	public GameObject VoidShieldTurnOnRight;

	public GameObject VoidShieldTurnOff;

	public GameObject VoidShieldTurnOffFront;

	public GameObject VoidShieldTurnOffBack;

	public GameObject VoidShieldTurnOffLeft;

	public GameObject VoidShieldTurnOffRight;

	public GameObject VoidShieldHit;

	[NotNull]
	public ShieldHitEntry[] ShieldHitEntrys = new ShieldHitEntry[0];

	public float RagdollDistanceForLootBag = 1.5f;

	[Tooltip("Шанс расчленёнки при смерти. 0 - вообще никогда не расчленяем / взрываем (только анимация смерти).")]
	[Range(0f, 100f)]
	public float GlobalDismembermentChance = 50f;

	[Tooltip("Если 0 то враги всегда взрываются, если 100 то всегда пытаемся отрезать конечности")]
	[Range(0f, 100f)]
	public float LimbsApartDismembermentChance = 80f;

	private bool m_Initialized;

	private HitCollection[] m_CachedDamageTypes;

	private HitCollection[] m_CachedEnergyTypes;

	private HitCollection[] m_CachedBillboardBloodTypes;

	private HitCollection[] m_CachedDirectionalBloodTypes;

	private HitCollection[] m_CachedBillboardAdditiveBloodTypes;

	private BloodPrefabsFromWeaponAnimationStyleEntry[] m_CachedBloodPrefabsFromWeaponAnimationStyleEntries;

	private Dictionary<SurfaceType, HitEntry> m_CachedHitEntryBySurfaceType;

	private void Initialize()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			m_CachedDamageTypes = new HitCollection[EnumUtils.GetMaxValue<DamageType>() + 1];
			m_CachedBillboardBloodTypes = new HitCollection[EnumUtils.GetMaxValue<SurfaceType>() + 1];
			m_CachedDirectionalBloodTypes = new HitCollection[EnumUtils.GetMaxValue<SurfaceType>() + 1];
			m_CachedBillboardAdditiveBloodTypes = new HitCollection[EnumUtils.GetMaxValue<SurfaceType>() + 1];
			m_CachedBloodPrefabsFromWeaponAnimationStyleEntries = new BloodPrefabsFromWeaponAnimationStyleEntry[EnumUtils.GetMaxValue<WeaponAnimationStyle>() + 1];
			m_CachedHitEntryBySurfaceType = new Dictionary<SurfaceType, HitEntry>(HitEffects.Length);
			DamageEntry[] damageTypes = DamageTypes;
			foreach (DamageEntry damageEntry in damageTypes)
			{
				m_CachedDamageTypes[(int)damageEntry.Type] = damageEntry.Hits;
			}
			HitEntry[] hitEffects = HitEffects;
			foreach (HitEntry hitEntry in hitEffects)
			{
				m_CachedBillboardBloodTypes[(int)hitEntry.Type] = hitEntry.CreaturesHitEffect.Billboard;
				m_CachedDirectionalBloodTypes[(int)hitEntry.Type] = hitEntry.CreaturesHitEffect.Directional;
				m_CachedBillboardAdditiveBloodTypes[(int)hitEntry.Type] = hitEntry.CreaturesHitEffect.BillboardAdditive;
				m_CachedHitEntryBySurfaceType.TryAdd(hitEntry.Type, hitEntry);
			}
			BloodPrefabsFromWeaponAnimationStyleEntry[] overrideHitDirectionPrefabFromAnimationStyle = OverrideHitDirectionPrefabFromAnimationStyle;
			foreach (BloodPrefabsFromWeaponAnimationStyleEntry bloodPrefabsFromWeaponAnimationStyleEntry in overrideHitDirectionPrefabFromAnimationStyle)
			{
				m_CachedBloodPrefabsFromWeaponAnimationStyleEntries[(int)bloodPrefabsFromWeaponAnimationStyleEntry.WeaponAnimationStyle] = bloodPrefabsFromWeaponAnimationStyleEntry;
			}
		}
	}

	[CanBeNull]
	public HitCollection GetDamage([NotNull] DamageData damage)
	{
		Initialize();
		return m_CachedDamageTypes[(int)damage.Type];
	}

	[CanBeNull]
	public GameObject GetBillboardBlood(SurfaceType type, HitLevel level, bool main)
	{
		Initialize();
		if (main)
		{
			return m_CachedBillboardBloodTypes[(int)type]?.Select(level);
		}
		return m_CachedBillboardAdditiveBloodTypes[(int)type]?.Select(level);
	}

	[CanBeNull]
	public GameObject GetDirectionalBlood(SurfaceType type, HitLevel level)
	{
		Initialize();
		return m_CachedDirectionalBloodTypes[(int)type]?.Select(level);
	}

	[CanBeNull]
	public GameObject GetBloodPuddle(SurfaceType type)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			return value?.CreaturesHitEffect.BloodPuddleLink.Load();
		}
		return null;
	}

	[CanBeNull]
	public GameObject GetDismember(SurfaceType type, UnitDismemberType dismemberType)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			return dismemberType switch
			{
				UnitDismemberType.Normal => value?.CreaturesHitEffect.DismemberLink.Load(), 
				UnitDismemberType.InPower => value?.CreaturesHitEffect.DismemberLink.Load(), 
				UnitDismemberType.LimbsApart => value?.CreaturesHitEffect.RipLimbsApartLink.Load(), 
				_ => null, 
			};
		}
		return null;
	}

	[CanBeNull]
	public DroppedLoot GetDismemberLoot(SurfaceType type)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			GameObject gameObject = value?.CreaturesHitEffect.DismemberLootLink.Load();
			if (gameObject == null)
			{
				return null;
			}
			return gameObject.GetComponent<DroppedLoot>();
		}
		return null;
	}

	[CanBeNull]
	public GameObject GetRipLimbsApart(SurfaceType type)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			return value?.CreaturesHitEffect.RipLimbsApartLink.Load();
		}
		return null;
	}

	[CanBeNull]
	public GameObject GetBleeding(SurfaceType type)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			return value?.CreaturesHitEffect?.BleedingLink?.Load();
		}
		return null;
	}

	public void PreloadCommonHits()
	{
		DamageEntry[] damageTypes = DamageTypes;
		for (int i = 0; i < damageTypes.Length; i++)
		{
			damageTypes[i].Hits.PreloadResources();
		}
	}

	public void WarmupCommonHits()
	{
		DamageEntry[] damageTypes = DamageTypes;
		foreach (DamageEntry damageEntry in damageTypes)
		{
			Warmup(damageEntry.Hits);
		}
	}

	private void Warmup(HitCollection c)
	{
		GameObjectsPool.Warmup(c.StandardLink.Load()?.GetComponent<PooledGameObject>(), 2);
		GameObjectsPool.Warmup(c.MinorLink.Load().GetComponent<PooledGameObject>(), 1);
		GameObjectsPool.Warmup(c.MajorLink.Load().GetComponent<PooledGameObject>(), 1);
		GameObjectsPool.Warmup(c.CritLink.Load().GetComponent<PooledGameObject>(), 1);
	}

	public Color GetBloodTypeColor(SurfaceType type, bool isDead = false)
	{
		Initialize();
		Color result = Color.magenta;
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			result = (isDead ? value.CreaturesHitEffect.DeadBloodColor : value.CreaturesHitEffect.BloodColor);
		}
		return result;
	}

	public Vector2 GetBloodTextureTilingSize(SurfaceType type)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			return value?.CreaturesHitEffect?.DefaultTileSize ?? Vector2.one;
		}
		return Vector2.one;
	}

	[NotNull]
	public AnimationCurve GetBloodFadeoutCurve(SurfaceType type)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			return value?.CreaturesHitEffect?.BloodFadeout ?? AnimationCurve.Linear(0f, 1f, 1f, 1f);
		}
		return AnimationCurve.Linear(0f, 1f, 1f, 1f);
	}

	[CanBeNull]
	public Texture2D GetBloodTexture(SurfaceType type)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			return value?.CreaturesHitEffect?.BloodTexture;
		}
		return null;
	}

	[CanBeNull]
	public StaticHitEffects GetStaticHitEffects(SurfaceType type)
	{
		Initialize();
		if (m_CachedHitEntryBySurfaceType.TryGetValue(type, out var value))
		{
			return value?.StaticHitEffects;
		}
		return null;
	}

	[CanBeNull]
	public DamageHitSettings DamageHitSettingsOfWeaponAnimationStyle(WeaponAnimationStyle style)
	{
		Initialize();
		return m_CachedBloodPrefabsFromWeaponAnimationStyleEntries[(int)style]?.ToDamageHitSettings(this) ?? DefaultDamage;
	}
}
