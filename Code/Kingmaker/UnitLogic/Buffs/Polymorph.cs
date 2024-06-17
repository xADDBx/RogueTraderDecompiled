using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI;
using Kingmaker.UI.Selection.UnitMark;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Particles;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Buffs;

[Serializable]
[TypeId("d9b639356e7149c68b313198122a72a3")]
public class Polymorph : UnitBuffComponentDelegate, IUnitSpawnHandler<EntitySubscriber>, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitSpawnHandler, EntitySubscriber>, IHashable
{
	[Serializable]
	public class VisualTransitionSettings
	{
		public float OldPrefabVisibilityTime = 0.5f;

		[AssetPicker("Assets/FX")]
		public GameObject OldPrefabFX;

		[AssetPicker("Assets/FX")]
		public GameObject NewPrefabFX;

		[ShowIf("HasScale")]
		public float ScaleTime = 0.5f;

		public bool ScaleOldPrefab = true;

		[ShowIf("ScaleOldPrefab")]
		public AnimationCurve OldScaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public bool ScaleNewPrefab = true;

		[ShowIf("ScaleNewPrefab")]
		public AnimationCurve NewScaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public bool HasScale
		{
			get
			{
				if (!ScaleOldPrefab)
				{
					return ScaleNewPrefab;
				}
				return true;
			}
		}
	}

	[FormerlySerializedAs("Prefab")]
	[SerializeField]
	[ValidateNotNull]
	private UnitViewLink m_Prefab;

	[SerializeField]
	private UnitViewLink m_PrefabFemale;

	[SerializeField]
	private BlueprintUnitReference m_ReplaceUnitForInspection;

	[SerializeField]
	private BlueprintPortraitReference m_Portrait;

	[SerializeField]
	private bool m_KeepSlots = true;

	public Size Size;

	[HideIf("m_KeepSlots")]
	public int StrengthBonus;

	[HideIf("m_KeepSlots")]
	public int AgilityBonus;

	[HideIf("m_KeepSlots")]
	public int ConstitutionBonus;

	[HideIf("m_KeepSlots")]
	public int NaturalArmor;

	[SerializeField]
	[HideIf("m_KeepSlots")]
	private BlueprintItemWeaponReference m_MainHand;

	[SerializeField]
	[HideIf("m_KeepSlots")]
	private BlueprintItemWeaponReference m_OffHand;

	[SerializeField]
	[HideIf("m_KeepSlots")]
	private BlueprintItemWeaponReference[] m_AdditionalLimbs;

	[SerializeField]
	[HideIf("m_KeepSlots")]
	private BlueprintItemWeaponReference[] m_SecondaryAdditionalLimbs;

	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts;

	[SerializeField]
	[HideIf("HasExternalTransition")]
	private VisualTransitionSettings m_EnterTransition;

	[SerializeField]
	[HideIf("HasExternalTransition")]
	private VisualTransitionSettings m_ExitTransition;

	[SerializeField]
	private PolymorphTransitionSettings m_TransitionExternal;

	[SerializeField]
	private bool m_SilentCaster = true;

	public bool KeepSlots => m_KeepSlots;

	public BlueprintUnit ReplaceUnitForInspection => m_ReplaceUnitForInspection?.Get();

	[CanBeNull]
	public BlueprintPortrait Portrait => m_Portrait;

	public BlueprintItemWeapon MainHand => m_MainHand?.Get();

	public BlueprintItemWeapon OffHand => m_OffHand?.Get();

	public ReferenceArrayProxy<BlueprintItemWeapon> AdditionalLimbs
	{
		get
		{
			BlueprintReference<BlueprintItemWeapon>[] additionalLimbs = m_AdditionalLimbs;
			return additionalLimbs;
		}
	}

	public ReferenceArrayProxy<BlueprintItemWeapon> SecondaryAdditionalLimbs
	{
		get
		{
			BlueprintReference<BlueprintItemWeapon>[] secondaryAdditionalLimbs = m_SecondaryAdditionalLimbs;
			return secondaryAdditionalLimbs;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	private bool HasExternalTransition => m_TransitionExternal;

	public bool SilentCaster => m_SilentCaster;

	public Size GetUnitSize(EntityFactComponent runtime)
	{
		return Size;
	}

	public UnitViewLink GetPrefab(AbstractUnitEntity unit)
	{
		if (unit.Gender != 0)
		{
			if (!m_PrefabFemale.Exists())
			{
				return m_Prefab;
			}
			return m_PrefabFemale;
		}
		return m_Prefab;
	}

	protected override void OnActivate()
	{
		PartPolymorphed optional = base.Owner.GetOptional<PartPolymorphed>();
		if (optional != null)
		{
			PFLog.Default.Error("Can't apply two polymorph effects to one character");
			return;
		}
		optional = base.Owner.GetOrCreate<PartPolymorphed>();
		optional.Setup(this);
		foreach (BlueprintUnitFact fact in Facts)
		{
			base.Owner.AddFact(fact).AddSource(base.Fact, this);
		}
		base.Owner.Body.ApplyPolymorphEffect(MainHand, OffHand, AdditionalLimbs.ToArray(), SecondaryAdditionalLimbs.ToArray(), m_KeepSlots);
		if (Portrait != null)
		{
			optional.OriginalPortrait = base.Owner.UISettings.PortraitBlueprintRaw;
			optional.OriginalPortraitData = base.Owner.UISettings.CustomPortraitRaw;
			optional.RestorePortrait = true;
			base.Owner.UISettings.SetPortrait(Portrait);
		}
		if (base.Owner.HoldingState != null)
		{
			TryReplaceView();
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		PartPolymorphed optional = base.Owner.GetOptional<PartPolymorphed>();
		if (optional != null && optional.Component != null && optional.Component != this)
		{
			PFLog.Default.Error("Can't apply two polymorph effects to one character");
			return;
		}
		optional = base.Owner.GetOrCreate<PartPolymorphed>();
		optional.Setup(this);
		int strengthBonus = StrengthBonus;
		int agilityBonus = AgilityBonus;
		int constitutionBonus = ConstitutionBonus;
		strengthBonus = AddStatBonus.TryApplyArcanistPowerfulChange(base.Context, StatType.WarhammerStrength, strengthBonus);
		agilityBonus = AddStatBonus.TryApplyArcanistPowerfulChange(base.Context, StatType.WarhammerAgility, agilityBonus);
		constitutionBonus = AddStatBonus.TryApplyArcanistPowerfulChange(base.Context, StatType.WarhammerToughness, constitutionBonus);
		base.Owner.GetOrCreate<UnitPartSizeModifier>().Add(base.Fact);
		base.Owner.Attributes.WarhammerStrength.AddModifier(strengthBonus, base.Runtime, ModifierDescriptor.Polymorph);
		base.Owner.Attributes.WarhammerAgility.AddModifier(agilityBonus, base.Runtime, ModifierDescriptor.Polymorph);
		base.Owner.Attributes.WarhammerToughness.AddModifier(constitutionBonus, base.Runtime, ModifierDescriptor.Polymorph);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartSizeModifier>()?.Remove(base.Fact);
		base.Owner.Body.CancelPolymorphEffect();
		PartPolymorphed optional = base.Owner.GetOptional<PartPolymorphed>();
		if (optional != null && optional.RestorePortrait)
		{
			base.Owner.UISettings.SetPortraitUnsafe(optional.OriginalPortrait, optional.OriginalPortraitData);
		}
		base.Owner.Attributes.WarhammerStrength.RemoveModifiersFrom(base.Runtime);
		base.Owner.Attributes.WarhammerAgility.RemoveModifiersFrom(base.Runtime);
		base.Owner.Attributes.WarhammerToughness.RemoveModifiersFrom(base.Runtime);
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
		base.Owner.Remove<PartPolymorphed>();
		if ((bool)optional?.ViewReplacement)
		{
			RestoreView();
		}
	}

	private void TryReplaceView(bool immediate = false)
	{
		PartPolymorphed optional = base.Owner.GetOptional<PartPolymorphed>();
		if (optional == null || (bool)optional.ViewReplacement || !base.Owner.View)
		{
			return;
		}
		UnitEntityView unitEntityView = GetPrefab(base.Owner).Load();
		if (unitEntityView == null)
		{
			return;
		}
		foreach (Buff buff in base.Owner.Buffs)
		{
			buff.ClearParticleEffect();
		}
		UnitEntityView view = base.Owner.View;
		UnitEntityView component = BlueprintComponent.Instantiate(unitEntityView).GetComponent<UnitEntityView>();
		component.UniqueId = base.Owner.UniqueId;
		UnityEngine.SceneManagement.Scene scene = view.gameObject.scene;
		SceneManager.MoveGameObjectToScene(component.gameObject, scene);
		component.ViewTransform.position = view.ViewTransform.position;
		component.ViewTransform.rotation = view.ViewTransform.rotation;
		component.DisableSizeScaling = true;
		component.Blueprint = base.Owner.Blueprint;
		base.Owner.AttachToViewOnLoad(component);
		optional.ViewReplacement = component.gameObject;
		base.Owner.Commands.InterruptAll((AbstractUnitCommand cmd) => !(cmd is UnitMoveTo));
		UIAccess.SelectionManager?.ForceCreateMarks();
		if (immediate)
		{
			UnityEngine.Object.Destroy(view.gameObject);
		}
		else
		{
			MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(Transition(HasExternalTransition ? m_TransitionExternal.EnterTransition : m_EnterTransition, view, component));
		}
		Game.Instance.SelectionCharacter.ReselectCurrentUnit();
	}

	private void RestoreView()
	{
		foreach (Buff buff in base.Owner.Buffs)
		{
			buff.ClearParticleEffect();
		}
		UnitEntityView view = base.Owner.View;
		UnitEntityView view2 = base.Owner.ViewSettings.Instantiate(ignorePolymorph: true);
		base.Owner.AttachView(view2);
		UnityEngine.SceneManagement.Scene scene = view.ViewTransform.gameObject.scene;
		SceneManager.MoveGameObjectToScene(base.Owner.View.ViewTransform.gameObject, scene);
		base.Owner.View.ViewTransform.position = view.ViewTransform.position;
		base.Owner.View.ViewTransform.rotation = view.ViewTransform.rotation;
		base.Owner.Commands.InterruptAll((AbstractUnitCommand cmd) => !(cmd is UnitMoveTo));
		ObjectExtensions.Or(UIAccess.SelectionManager, null)?.ForceCreateMarks();
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(Transition(HasExternalTransition ? m_TransitionExternal.ExitTransition : m_ExitTransition, view, base.Owner.View));
		Game.Instance.SelectionCharacter.ReselectCurrentUnit();
	}

	public static IEnumerator Transition(VisualTransitionSettings settings, UnitEntityView oldView, UnitEntityView newView)
	{
		List<BaseUnitMark> list = ListPool<BaseUnitMark>.Claim();
		oldView.GetComponentsInChildren(list);
		foreach (BaseUnitMark item in list)
		{
			item.gameObject.SetActive(value: false);
		}
		UnitAnimationManager unitAnimationManager = ObjectExtensions.Or(oldView.AnimationManager, null);
		if ((bool)unitAnimationManager)
		{
			unitAnimationManager.StopEvents();
			unitAnimationManager.Disabled = true;
		}
		UnityEngine.Object.Destroy(oldView.gameObject, settings.OldPrefabVisibilityTime);
		if ((bool)settings.OldPrefabFX)
		{
			FxHelper.SpawnFxOnGameObject(settings.OldPrefabFX, oldView.gameObject);
		}
		if ((bool)settings.NewPrefabFX)
		{
			FxHelper.SpawnFxOnGameObject(settings.NewPrefabFX, newView.gameObject);
		}
		if (!settings.HasScale)
		{
			yield break;
		}
		newView.ParticlesSnapMap.RestoreBoneTransforms();
		bool doNotAdjustScale = true;
		newView.DoNotAdjustScale = true;
		oldView.DoNotAdjustScale = doNotAdjustScale;
		float? oldCorpulence = oldView.ParticlesSnapMap["Locator_TorsoCenterFX"]?.Transform?.lossyScale.x;
		float? newCorpulence = newView.ParticlesSnapMap["Locator_TorsoCenterFX"]?.Transform?.lossyScale.x;
		if (!oldCorpulence.HasValue || !newCorpulence.HasValue)
		{
			oldCorpulence = oldView.Corpulence + 1f;
			newCorpulence = newView.Corpulence + 1f;
		}
		float d = settings.ScaleTime;
		double start = Game.Instance.TimeController.RealTime.TotalSeconds;
		Vector3 oldOriginalScale = oldView.ViewTransform.localScale;
		Vector3 newOriginalScale = newView.ViewTransform.localScale;
		do
		{
			float time = (float)(Game.Instance.TimeController.RealTime.TotalSeconds - start) / d;
			if (settings.ScaleOldPrefab && (bool)oldView)
			{
				float t = settings.OldScaleCurve.Evaluate(time);
				float num = Mathf.Lerp(1f, newCorpulence.Value / oldCorpulence.Value, t);
				oldView.ViewTransform.localScale = oldOriginalScale * num;
			}
			if (settings.ScaleNewPrefab && (bool)newView)
			{
				float t2 = settings.NewScaleCurve.Evaluate(time);
				float num2 = Mathf.Lerp(oldCorpulence.Value / newCorpulence.Value, 1f, t2);
				newView.ViewTransform.localScale = newOriginalScale * num2;
			}
			yield return null;
		}
		while (Game.Instance.TimeController.RealTime.TotalSeconds - start < (double)d);
		if ((bool)newView)
		{
			newView.ViewTransform.localScale = newOriginalScale;
			newView.DoNotAdjustScale = false;
		}
	}

	public void HandleUnitSpawned()
	{
		TryReplaceView(immediate: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
