using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.Mechanics;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Decals;
using Kingmaker.Visual.FX.BleedingEffect;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;
using Kingmaker.Visual.MaterialEffects.ColorTint;
using Kingmaker.Visual.MaterialEffects.CustomMaterialProperty;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Kingmaker.Visual.MaterialEffects.LayeredMaterial;
using Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;
using Kingmaker.Visual.MaterialEffects.RimLighting;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public static class FxHelper
{
	private class BloodKeeper : MonoBehaviour
	{
		public readonly HashSet<GameObject> List = new HashSet<GameObject>();
	}

	private static Transform s_FxRoot;

	private static Transform s_FootprintsRoot;

	private static BloodKeeper s_Blood;

	private static HashSet<GameObject> BloodList
	{
		get
		{
			if (!s_Blood)
			{
				s_Blood = FxRoot.GetComponent<BloodKeeper>();
			}
			return s_Blood.List;
		}
	}

	public static Transform FxRoot
	{
		get
		{
			if (!s_FxRoot)
			{
				GameObject gameObject = new GameObject("[Fx Root]");
				Game.Instance.DynamicRoot?.Add(gameObject.transform);
				s_FxRoot = gameObject.transform;
				s_Blood = gameObject.EnsureComponent<BloodKeeper>();
			}
			return s_FxRoot;
		}
	}

	public static Transform FootprintsRoot
	{
		get
		{
			if (s_FootprintsRoot == null)
			{
				GameObject gameObject = new GameObject("[Footprints Root]");
				Game.Instance.DynamicRoot?.Add(gameObject.transform);
				s_FootprintsRoot = gameObject.transform;
			}
			return s_FootprintsRoot;
		}
	}

	public static GameObject SpawnFxOnGameObject(GameObject prefab, GameObject target, float snapToLocatorRaceScale = 1f, bool enableFxObject = true, Quaternion? overriddenRotationSource = null)
	{
		try
		{
			if (!prefab || !target)
			{
				return null;
			}
			Vector3 position = target.transform.position;
			Quaternion rotation = overriddenRotationSource ?? target.transform.rotation;
			GameObject gameObject = InstantiateFx(prefab, position, rotation, enableFxObject);
			ResetPlayOnAwake(gameObject);
			gameObject.transform.parent = FxRoot;
			SnapMapBase component = target.GetComponent<SnapMapBase>();
			if (component != null)
			{
				if (!component.Initialized)
				{
					component.Init();
				}
				List<SnapControllerBase> list = ListPool<SnapControllerBase>.Claim();
				gameObject.GetComponentsInChildren(list);
				foreach (SnapControllerBase item in list)
				{
					item.Play(component);
				}
				ListPool<SnapControllerBase>.Release(list);
				List<SnapToLocator> list2 = ListPool<SnapToLocator>.Claim();
				gameObject.GetComponentsInChildren(list2);
				foreach (SnapToLocator item2 in list2)
				{
					if (item2.BoneNames.Count > 1)
					{
						for (int i = 1; i < item2.BoneNames.Count; i++)
						{
							string text = item2.BoneNames[i];
							if (!string.IsNullOrEmpty(text))
							{
								GameObject gameObject2 = Object.Instantiate(item2.gameObject, position, Quaternion.identity);
								gameObject2.transform.parent = FxRoot;
								SnapToLocator component2 = gameObject2.GetComponent<SnapToLocator>();
								component2.BoneNames.Clear();
								component2.BoneName = text;
								component2.Attach(component, target.transform);
								EnableAutoDestroy(gameObject2);
							}
						}
					}
					item2.RaceScale = snapToLocatorRaceScale;
					item2.Attach(component, target.transform);
				}
				ListPool<SnapToLocator>.Release(list2);
			}
			ApplyMaterialAnimations(target.gameObject, gameObject);
			CheckDecals(gameObject);
			CheckBillboards(gameObject);
			List<HighlightAnimation> list3 = ListPool<HighlightAnimation>.Claim();
			gameObject.GetComponentsInChildren(list3);
			if (list3.Count > 0)
			{
				UnitMultiHighlight component3 = target.GetComponent<UnitMultiHighlight>();
				if ((bool)component3)
				{
					foreach (HighlightAnimation item3 in list3)
					{
						item3.Play(component3);
					}
				}
				else
				{
					PFLog.Default.Error(target, "UnitMultiHighlight is missing on " + target.name);
				}
			}
			ListPool<HighlightAnimation>.Release(list3);
			EnableAutoDestroy(gameObject);
			AbstractUnitEntityView componentNonAlloc = target.GetComponentNonAlloc<AbstractUnitEntityView>();
			if ((bool)componentNonAlloc)
			{
				UnitEntityView unitEntityView = componentNonAlloc as UnitEntityView;
				DismemberUnitFX componentNonAlloc2 = gameObject.GetComponentNonAlloc<DismemberUnitFX>();
				if ((bool)componentNonAlloc2)
				{
					componentNonAlloc2.Init(componentNonAlloc);
				}
				MirrorImageFX componentNonAlloc3 = gameObject.GetComponentNonAlloc<MirrorImageFX>();
				if ((bool)componentNonAlloc3 && unitEntityView != null)
				{
					componentNonAlloc3.Init(unitEntityView);
				}
				if (gameObject.GetComponentNonAlloc<FxIsAlwaysVisible>() == null && !componentNonAlloc.Blueprint.IsCheater)
				{
					gameObject.EnsureComponent<UnitFxVisibilityManager>().Init(componentNonAlloc);
				}
				FxProjectileLauncher componentNonAlloc4 = gameObject.GetComponentNonAlloc<FxProjectileLauncher>();
				if ((bool)componentNonAlloc4 && unitEntityView != null)
				{
					componentNonAlloc4.HitTarget = unitEntityView;
				}
				if (!target.TryGetComponent<FxAttachmentSlot>(out var component4))
				{
					component4 = target.AddComponent<FxAttachmentSlot>();
					component4.hideFlags |= HideFlags.DontSave;
				}
				if (!gameObject.TryGetComponent<FxAttachment>(out var component5))
				{
					component5 = gameObject.AddComponent<FxAttachment>();
					component5.hideFlags |= HideFlags.DontSave;
				}
				component5.Attach(component4);
			}
			List<IFxSpawner> list4 = ListPool<IFxSpawner>.Claim();
			gameObject.GetComponentsInChildren(list4);
			list4.Sort(SortByPriority);
			foreach (IFxSpawner item4 in list4)
			{
				item4.SpawnFxOnGameObject(target);
			}
			ListPool<IFxSpawner>.Release(list4);
			List<BleedingEffectSetup> list5 = ListPool<BleedingEffectSetup>.Claim();
			gameObject.GetComponentsInChildren(list5);
			if (list5.Count > 0)
			{
				UnitEntityView componentNonAlloc5 = target.GetComponentNonAlloc<UnitEntityView>();
				if (componentNonAlloc5 != null && componentNonAlloc5.EntityData != null)
				{
					SurfaceType surfaceType = componentNonAlloc5.EntityData.SurfaceType;
					GameObject bleeding = BlueprintRoot.Instance.HitSystemRoot.GetBleeding(surfaceType);
					if (bleeding != null)
					{
						ListPool<BleedingEffectSetup>.Release(list5);
						return SpawnFxOnGameObject(bleeding, target, snapToLocatorRaceScale);
					}
				}
				else
				{
					PFLog.Default.Error(target, "Can't get EntityData on " + target.name);
				}
			}
			ListPool<BleedingEffectSetup>.Release(list5);
			return gameObject;
		}
		finally
		{
		}
	}

	public static void Resnap(GameObject snapOwner, GameObject target, int snapperIndex)
	{
		if (!snapOwner || !target)
		{
			return;
		}
		SnapMapBase component = target.GetComponent<SnapMapBase>();
		if (component != null)
		{
			if (!component.Initialized)
			{
				component.Init();
			}
			List<SnapToLocator> list = ListPool<SnapToLocator>.Claim();
			snapOwner.GetComponentsInChildren(list);
			list.First((SnapToLocator s) => s.IndexForExtraSnapping == snapperIndex).Attach(component, target.transform);
			ListPool<SnapToLocator>.Release(list);
		}
	}

	private static int SortByPriority(IFxSpawner x, IFxSpawner y)
	{
		return x.Priority.CompareTo(y.Priority);
	}

	public static GameObject SpawnFxOnEntity(GameObject prefab, MechanicEntityView entity, bool enableFxObject = true, bool overrideOrientationSource = false)
	{
		if ((bool)prefab && entity is AbstractUnitEntityView abstractUnitEntityView)
		{
			float coeff = BlueprintRoot.Instance.FxRoot.RaceFxSnapToLocatorScaleSettings.GetCoeff(abstractUnitEntityView.Blueprint.Race?.RaceId);
			return SpawnFxOnGameObject(prefab, entity.gameObject, coeff, enableFxObject, (overrideOrientationSource && abstractUnitEntityView.HasOverriddenRotatablePart) ? new Quaternion?(abstractUnitEntityView.OverrideRotatablePart.transform.rotation) : null);
		}
		return null;
	}

	public static GameObject SpawnFxOnWeapon(GameObject prefab, MechanicEntityView unit, WeaponParticlesSnapMap weaponSnap)
	{
		if ((bool)prefab && (bool)unit && (bool)weaponSnap)
		{
			return SpawnFxOnGameObject(prefab, weaponSnap.gameObject);
		}
		return null;
	}

	private static void CheckDecals(GameObject fx)
	{
		FxDecal[] componentsInChildren = fx.GetComponentsInChildren<FxDecal>(includeInactive: true);
		foreach (FxDecal obj in componentsInChildren)
		{
			obj.AutoDestroy = true;
			obj.gameObject.SetActive(value: true);
		}
	}

	private static void CheckBillboards(GameObject fx)
	{
		List<Billboard> list = ListPool<Billboard>.Claim();
		fx.GetComponentsInChildren(list);
		foreach (Billboard item in list)
		{
			item.UpdateBillboard();
		}
		ListPool<Billboard>.Release(list);
	}

	private static void ApplyMaterialAnimations(GameObject target, GameObject particleEffect)
	{
		List<ColorTintAnimationSetup> list = ListPool<ColorTintAnimationSetup>.Claim();
		List<RimLightingAnimationSetup> list2 = ListPool<RimLightingAnimationSetup>.Claim();
		List<DissolveSetup> list3 = ListPool<DissolveSetup>.Claim();
		List<AdditionalAlbedoSetup> list4 = ListPool<AdditionalAlbedoSetup>.Claim();
		List<MaterialParametersOverrideSetup> list5 = ListPool<MaterialParametersOverrideSetup>.Claim();
		List<LayeredMaterialAnimationSetup> list6 = ListPool<LayeredMaterialAnimationSetup>.Claim();
		List<CustomMaterialPropertyAnimationSetup> list7 = ListPool<CustomMaterialPropertyAnimationSetup>.Claim();
		try
		{
			particleEffect.GetComponentsInChildren(list);
			particleEffect.GetComponentsInChildren(list2);
			particleEffect.GetComponentsInChildren(list3);
			particleEffect.GetComponentsInChildren(list4);
			particleEffect.GetComponentsInChildren(list5);
			particleEffect.GetComponentsInChildren(list7);
			particleEffect.GetComponentsInChildren(list6);
			if (list.Count <= 0 && list2.Count <= 0 && list3.Count <= 0 && list4.Count <= 0 && list5.Count <= 0 && list7.Count <= 0 && list6.Count <= 0)
			{
				return;
			}
			if (target != null)
			{
				StandardMaterialController standardMaterialController = target.GetComponent<StandardMaterialController>();
				if (standardMaterialController == null)
				{
					standardMaterialController = target.gameObject.AddComponent<StandardMaterialController>();
				}
				StopAnimationsOnDestroy stopAnimationsOnDestroy = particleEffect.EnsureComponent<StopAnimationsOnDestroy>();
				stopAnimationsOnDestroy.MaterialAnimator = standardMaterialController;
				foreach (ColorTintAnimationSetup item in list)
				{
					standardMaterialController.ColorTintController.Animations.Add(item.Settings);
					stopAnimationsOnDestroy.AddColorTintAnimationSettings(item.Settings);
				}
				foreach (RimLightingAnimationSetup item2 in list2)
				{
					standardMaterialController.RimController.Animations.Add(item2.Settings);
					stopAnimationsOnDestroy.AddRimLightAnimationSetting(item2.Settings);
				}
				foreach (DissolveSetup item3 in list3)
				{
					standardMaterialController.DissolveController.Animations.Add(item3.Settings);
					stopAnimationsOnDestroy.AddDissolveAnimationSettings(item3.Settings);
				}
				foreach (AdditionalAlbedoSetup item4 in list4)
				{
					standardMaterialController.AdditionalAlbedoController.Animations.Add(item4.Settings);
					stopAnimationsOnDestroy.AddPetrificationAnimationSettings(item4.Settings);
				}
				foreach (MaterialParametersOverrideSetup item5 in list5)
				{
					standardMaterialController.MaterialParametersOverrideController.Entries.Add(item5.Settings);
					stopAnimationsOnDestroy.AddMaterialParametersOverrideSettings(item5.Settings);
				}
				foreach (CustomMaterialPropertyAnimationSetup item6 in list7)
				{
					int token = standardMaterialController.CustomMaterialPropertyAnimationController.AddAnimation(item6);
					stopAnimationsOnDestroy.AddCustomPropertyAnimation(token);
				}
				{
					foreach (LayeredMaterialAnimationSetup item7 in list6)
					{
						if (standardMaterialController.TryAddOverlayAnimation(item7, out var token2))
						{
							stopAnimationsOnDestroy.AddLayeredMaterialAnimationToken(token2);
						}
					}
					return;
				}
			}
			foreach (ColorTintAnimationSetup item8 in list)
			{
				StandardMaterialController component = item8.GetComponent<StandardMaterialController>();
				if (component != null)
				{
					component.ColorTintController.Animations.Add(item8.Settings);
					StopAnimationsOnDestroy component2 = item8.GetComponent<StopAnimationsOnDestroy>();
					if (component2 != null)
					{
						component2.AddColorTintAnimationSettings(item8.Settings);
					}
				}
			}
			foreach (RimLightingAnimationSetup item9 in list2)
			{
				StandardMaterialController component3 = item9.GetComponent<StandardMaterialController>();
				if (component3 != null)
				{
					component3.RimController.Animations.Add(item9.Settings);
					StopAnimationsOnDestroy component4 = item9.GetComponent<StopAnimationsOnDestroy>();
					if (component4 != null)
					{
						component4.AddRimLightAnimationSetting(item9.Settings);
					}
				}
			}
			foreach (DissolveSetup item10 in list3)
			{
				StandardMaterialController component5 = item10.GetComponent<StandardMaterialController>();
				if (component5 != null)
				{
					component5.DissolveController.Animations.Add(item10.Settings);
					StopAnimationsOnDestroy component6 = item10.GetComponent<StopAnimationsOnDestroy>();
					if (component6 != null)
					{
						component6.AddDissolveAnimationSettings(item10.Settings);
					}
				}
			}
			foreach (AdditionalAlbedoSetup item11 in list4)
			{
				StandardMaterialController component7 = item11.GetComponent<StandardMaterialController>();
				if (component7 != null)
				{
					component7.AdditionalAlbedoController.Animations.Add(item11.Settings);
					StopAnimationsOnDestroy component8 = item11.GetComponent<StopAnimationsOnDestroy>();
					if (component8 != null)
					{
						component8.AddPetrificationAnimationSettings(item11.Settings);
					}
				}
			}
			foreach (MaterialParametersOverrideSetup item12 in list5)
			{
				StandardMaterialController component9 = item12.GetComponent<StandardMaterialController>();
				if (component9 != null)
				{
					component9.MaterialParametersOverrideController.Entries.Add(item12.Settings);
					StopAnimationsOnDestroy component10 = item12.GetComponent<StopAnimationsOnDestroy>();
					if (component10 != null)
					{
						component10.AddMaterialParametersOverrideSettings(item12.Settings);
					}
				}
			}
			foreach (CustomMaterialPropertyAnimationSetup item13 in list7)
			{
				if (item13.TryGetComponent<StandardMaterialController>(out var component11) && item13.TryGetComponent<StopAnimationsOnDestroy>(out var component12))
				{
					int token3 = component11.CustomMaterialPropertyAnimationController.AddAnimation(item13);
					component12.AddCustomPropertyAnimation(token3);
				}
			}
			foreach (LayeredMaterialAnimationSetup item14 in list6)
			{
				StandardMaterialController component13 = item14.GetComponent<StandardMaterialController>();
				if (!(component13 == null) && component13.TryAddOverlayAnimation(item14, out var token4) && item14.TryGetComponent<StopAnimationsOnDestroy>(out var component14))
				{
					component14.AddLayeredMaterialAnimationToken(token4);
				}
			}
		}
		finally
		{
			ListPool<ColorTintAnimationSetup>.Release(list);
			ListPool<RimLightingAnimationSetup>.Release(list2);
			ListPool<DissolveSetup>.Release(list3);
			ListPool<AdditionalAlbedoSetup>.Release(list4);
			ListPool<MaterialParametersOverrideSetup>.Release(list5);
			ListPool<LayeredMaterialAnimationSetup>.Release(list6);
			ListPool<CustomMaterialPropertyAnimationSetup>.Release(list7);
		}
	}

	public static GameObject SpawnFxOnPoint(GameObject prefab, Vector3 point, bool enableFxObject = true)
	{
		return SpawnFxOnPoint(prefab, point, Quaternion.identity, enableFxObject);
	}

	public static GameObject SpawnFxOnPoint(GameObject prefab, Vector3 point, Quaternion rotation, bool enableFxObject = true)
	{
		try
		{
			if (!prefab)
			{
				return null;
			}
			GameObject gameObject = InstantiateFx(prefab, point, rotation, enableFxObject);
			ResetPlayOnAwake(gameObject);
			gameObject.transform.parent = FxRoot;
			ApplyMaterialAnimations(null, gameObject);
			CheckDecals(gameObject);
			CheckBillboards(gameObject);
			EnableAutoDestroy(gameObject);
			List<IFxSpawner> list = ListPool<IFxSpawner>.Claim();
			gameObject.GetComponentsInChildren(list);
			list.Sort(SortByPriority);
			foreach (IFxSpawner item in list)
			{
				item.SpawnFxOnPoint(point, rotation);
			}
			ListPool<IFxSpawner>.Release(list);
			return gameObject;
		}
		finally
		{
		}
	}

	[NotNull]
	private static GameObject InstantiateFx([NotNull] GameObject prefab, Vector3 position, Quaternion rotation, bool enableFxObject = true)
	{
		return GameObjectsPool.Claim(prefab, position, rotation, enableFxObject);
	}

	private static void EnableAutoDestroy(GameObject particleEffect)
	{
		List<AutoDestroy> list = ListPool<AutoDestroy>.Claim();
		particleEffect.GetComponentsInChildren(list);
		foreach (AutoDestroy item in list)
		{
			item.enabled = true;
		}
		ListPool<AutoDestroy>.Release(list);
	}

	public static void Stop(GameObject fx)
	{
		if (!fx)
		{
			return;
		}
		List<FxDestroyOnStop> list = ListPool<FxDestroyOnStop>.Claim();
		fx.GetComponentsInChildren(list);
		foreach (FxDestroyOnStop item in list)
		{
			item.Stop();
		}
		ListPool<FxDestroyOnStop>.Release(list);
		List<ParticleSystem> list2 = ListPool<ParticleSystem>.Claim();
		fx.GetComponentsInChildren(list2);
		foreach (ParticleSystem item2 in list2)
		{
			item2.Stop();
		}
		ListPool<ParticleSystem>.Release(list2);
		List<ObjectRotator> list3 = ListPool<ObjectRotator>.Claim();
		fx.GetComponentsInChildren(list3);
		foreach (ObjectRotator item3 in list3)
		{
			item3.Stop();
		}
		ListPool<ObjectRotator>.Release(list3);
		List<RecursiveFx> list4 = ListPool<RecursiveFx>.Claim();
		fx.GetComponentsInChildren(list4);
		foreach (RecursiveFx item4 in list4)
		{
			item4.Stop();
		}
		ListPool<RecursiveFx>.Release(list4);
	}

	public static void Destroy(GameObject fx, bool immediate = false)
	{
		if (!(fx != null))
		{
			return;
		}
		if (immediate || !fx.TryGetComponent<FxFadeOut>(out var component) || component.Duration <= 0f)
		{
			GameObjectsPool.Release(fx);
		}
		else
		{
			component.StartFadeOut();
		}
		List<RecursiveFx> list = ListPool<RecursiveFx>.Claim();
		fx.GetComponentsInChildren(list);
		foreach (RecursiveFx item in list)
		{
			item.OnDestroyed(immediate);
		}
		ListPool<RecursiveFx>.Release(list);
	}

	public static void DestroyAll()
	{
		for (int num = FxRoot.childCount - 1; num >= 0; num--)
		{
			GameObjectsPool.Release(FxRoot.GetChild(num).gameObject);
		}
		BloodList.Clear();
	}

	public static void DestroyAllBlood()
	{
		foreach (GameObject blood in BloodList)
		{
			if (!(blood == null) && blood.activeInHierarchy)
			{
				GameObjectsPool.Release(blood);
			}
		}
		BloodList.Clear();
	}

	public static void RegisterBlood(GameObject bloodFx)
	{
		BloodList.Add(bloodFx);
	}

	private static void ResetPlayOnAwake(GameObject fx)
	{
		List<ParticleSystem> list = ListPool<ParticleSystem>.Claim();
		fx.GetComponentsInChildren(list);
		foreach (ParticleSystem item in list)
		{
			ParticleSystem.MainModule main = item.main;
			main.playOnAwake = false;
		}
		ListPool<ParticleSystem>.Release(list);
	}
}
