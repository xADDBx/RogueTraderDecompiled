using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public class UnitUseAbility : UnitCommand<UnitUseAbilityParams>
{
	[JsonProperty]
	private UnitAnimationActionCastSpell.CastAnimationStyle m_CastAnimStyle;

	[JsonProperty]
	private float m_CastTime;

	[JsonProperty]
	private bool m_AfterPrecastFxSpawned;

	[JsonProperty]
	private List<GameObject> m_HandFxObjects;

	[JsonProperty]
	private List<GameObject> m_GroundFxObjects;

	[JsonProperty]
	private UnitAnimationActionCastSpell.SpecialBehaviourType m_Special;

	[JsonProperty]
	private bool m_RotateIsDone;

	[JsonProperty]
	public static bool TestPauseOnCast { get; set; }

	public AbilityExecutionProcess ExecutionProcess { get; private set; }

	public bool IgnoreCooldown => base.Params.IgnoreCooldown;

	[NotNull]
	public AbilityData Ability => base.Params.Ability;

	public bool IgnoreAbilityUsingInThreateningArea => base.Params.IgnoreAbilityUsingInThreateningArea;

	public bool IsInstantDeliver => Ability.IsInstantDeliver;

	public int ActionsCount => Ability.ActionsCount;

	public int CurrentActionIndex => ExecutionProcess?.Context.ActionIndex ?? 0;

	public bool ForceCastOnBadTarget
	{
		get
		{
			if (!base.FromCutscene)
			{
				return (base.Target?.Entity?.IsCheater).GetValueOrDefault();
			}
			return true;
		}
	}

	private bool IsTargetingDeadUnit
	{
		get
		{
			MechanicEntity mechanicEntity = base.Target?.Entity;
			if (mechanicEntity != null && mechanicEntity.IsDead && !Ability.Blueprint.CanCastToDeadTarget)
			{
				return !Ability.Blueprint.CanTargetPoint;
			}
			return false;
		}
	}

	private WeaponAnimationStyle WeaponStyle
	{
		get
		{
			UnitViewHandsEquipment handsEquipment = base.Executor.View.HandsEquipment;
			if (handsEquipment != null && !handsEquipment.InCombat)
			{
				return WeaponAnimationStyle.Fist;
			}
			if (Ability.Weapon != null)
			{
				return base.Executor.View.HandsEquipment?.GetWeaponStyleForWeapon(Ability.Weapon) ?? Ability.Weapon.GetAnimationStyle();
			}
			return base.Executor.View.HandsEquipment?.GetWeaponStyleForHand(base.Executor.Body.PrimaryHand) ?? WeaponAnimationStyle.None;
		}
	}

	public bool DisableLog => base.Params.DisableLog;

	public AttackHitPolicyType HitPolicy => base.Params.HitPolicy;

	public DamagePolicyType DamagePolicy => base.Params.DamagePolicy;

	public bool KillTarget => base.Params.KillTarget;

	public bool RotateIsDone => m_RotateIsDone;

	public bool SyncRotationAndAttack
	{
		get
		{
			if (ShouldTurnToTarget)
			{
				return Ability.Blueprint.SyncRotationAndAttack;
			}
			return false;
		}
	}

	public override bool ShouldBeInterrupted
	{
		get
		{
			if (!base.IsActed)
			{
				if (!IsTargetingDeadUnit && !base.Executor.State.IsProne)
				{
					return base.Executor.IsDeadOrUnconscious;
				}
				return true;
			}
			return false;
		}
	}

	public override bool IsUnitEnoughClose
	{
		get
		{
			if (base.Target == null || Ability.IsRangeUnrestrictedForTarget(base.Target))
			{
				return true;
			}
			if (!base.FromCutscene && !(Ability.TryGetCasterForDistanceCalculation(out var caster) ? caster : base.Executor).InRangeInCells(base.Target, Ability.RangeCells))
			{
				return false;
			}
			CustomGridNode currentUnwalkableNode = base.Executor.CurrentUnwalkableNode;
			int distance;
			LosCalculations.CoverType los;
			if (base.NeedLoS)
			{
				return Ability.CanTargetFromNode(currentUnwalkableNode, null, base.Target, out distance, out los);
			}
			return true;
		}
	}

	public override bool IsMoveUnit => Ability.Blueprint.IsMoveUnit;

	public override bool NeedEquipWeapons
	{
		get
		{
			if (!(Ability.SourceItem is ItemEntityWeapon))
			{
				return Ability.Blueprint.NeedEquipWeapons;
			}
			return true;
		}
	}

	public override bool DontWaitForHands => Ability.Blueprint.GetComponent<AbilityCustomCharge>();

	public override bool ShouldTurnToTarget => Ability.Blueprint.ShouldTurnToTarget;

	public override bool MarkManualTarget => false;

	public override bool IsInterruptible
	{
		get
		{
			if (ExecutionProcess == null || !ExecutionProcess.IsEngageUnit)
			{
				return base.IsInterruptible;
			}
			return false;
		}
	}

	public override bool CanStart
	{
		get
		{
			if (!base.IsFreeAction && !base.FromCutscene)
			{
				return Ability.HasEnoughActionPoint;
			}
			return true;
		}
	}

	protected override int ExpectedActEventsCount => ActionsCount;

	protected override float PretendActDelay
	{
		get
		{
			if (!Ability.Blueprint.TryGetComponent<AbilityNoCastAnimation>(out var component))
			{
				return 1f;
			}
			return component.PretendActDelay;
		}
	}

	public UnitUseAbility([NotNull] UnitUseAbilityParams @params)
		: base(@params)
	{
		m_RotateIsDone = true;
	}

	protected void ForceImmediateAction()
	{
		ResultType resultType = OnAction();
		if (resultType != 0)
		{
			ForceFinish(resultType);
		}
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		base.OnInit(executor);
		if (!base.FromCutscene && !Ability.CanTarget(base.Target, out var unavailableReason))
		{
			PFLog.Default.Error($"{Ability.Blueprint.NameSafe()}: cannot target {base.Target} because of {unavailableReason}. Cast by {base.Executor}");
			QAModeExceptionReporter.MaybeShowError($"{Ability.Blueprint.NameSafe()}: cannot target {base.Target} because of {unavailableReason}. Cast by {base.Executor}");
		}
		if (!base.FromCutscene)
		{
			Ability fact = Ability.Fact;
			if (fact != null && !fact.Active)
			{
				PFLog.Default.Error(base.Executor.View, $"Unit {base.Executor} casting spell {Ability.Blueprint} that is not in spellbook or unit abilities");
				Interrupt();
				return;
			}
		}
		if (!base.IsOneFrameCommand)
		{
			m_CastAnimStyle = Ability.Blueprint.Animation;
			m_CastTime = 2.5f;
		}
		else
		{
			m_Special = UnitAnimationActionCastSpell.SpecialBehaviourType.NoPrecast;
			m_CastTime = 0f;
		}
		BlueprintItemEquipmentUsable sourceItemUsableBlueprint = Ability.SourceItemUsableBlueprint;
		if (sourceItemUsableBlueprint != null && sourceItemUsableBlueprint.Type == UsableItemType.Wand)
		{
			m_CastAnimStyle = ((base.Target.Entity == base.Executor) ? UnitAnimationActionCastSpell.CastAnimationStyle.Self : UnitAnimationActionCastSpell.CastAnimationStyle.Directional);
			m_Special = UnitAnimationActionCastSpell.SpecialBehaviourType.NoPrecast;
			m_CastTime = 0f;
		}
		if (TurnController.IsInTurnBasedCombat() && base.Executor.IsInCombat)
		{
			m_CastTime = Math.Min(5f, m_CastTime);
		}
		if (m_CastAnimStyle == UnitAnimationActionCastSpell.CastAnimationStyle.Medicine)
		{
			m_CastAnimStyle = ((base.Target?.Entity == base.Executor) ? UnitAnimationActionCastSpell.CastAnimationStyle.MedicineSelf : UnitAnimationActionCastSpell.CastAnimationStyle.Medicine);
		}
	}

	protected override void TriggerAnimation()
	{
		AbilityData ability = Ability;
		if ((object)ability != null && !ability.SourceItemIsWeapon && ability.OverrideWeapon == null && Ability.Blueprint.GetComponent<AbilityCustomBladeDance>() == null)
		{
			base.Executor.View.HideOffWeapon(hide: true);
		}
		TryStopBuffLoopAction();
		AbilityCustomBladeDance component = Ability.Blueprint.GetComponent<AbilityCustomBladeDance>();
		if (component != null && !component.UseOnSourceWeapon)
		{
			Ability.OverrideWeapon = (component.UseSecondWeapon ? (base.Executor.GetSecondaryHandWeapon() ?? base.Executor.GetFirstWeapon()) : ((base.Executor.GetFirstWeapon() != null && component.UseSpecificWeaponClassification && base.Executor.GetFirstWeapon()?.Blueprint.Classification == component.Classification) ? base.Executor.GetFirstWeapon() : ((component.UseSpecificWeaponClassification && base.Executor.GetSecondaryHandWeapon()?.Blueprint.Classification == component.Classification) ? base.Executor.GetSecondaryHandWeapon() : base.Executor.GetFirstWeapon())));
		}
		if (Ability.Blueprint.GetComponent<UseCurrentWeaponAnimation>() != null && Ability.Blueprint.GetComponent<UseCurrentWeaponAnimation>()?.GetWeapon(base.Executor) != null)
		{
			Ability.OverrideWeapon = Ability.Blueprint.GetComponent<UseCurrentWeaponAnimation>()?.GetWeapon(base.Executor);
		}
		if (base.Target != null)
		{
			Vector3 vector = base.Target.Point - base.Executor.Position;
			BlueprintAbilityFXSettings fXSettings = Ability.FXSettings;
			if (fXSettings != null && fXSettings.ShouldOffsetTargetRelativePosition)
			{
				Vector3 vector2 = Quaternion.LookRotation((base.ApproachPoint - base.Executor.Position).normalized) * Ability.FXSettings.OffsetTargetPosition;
				vector = base.ApproachPoint + vector2;
				if (base.Executor.MaybeAnimationManager != null)
				{
					base.Executor.MaybeAnimationManager.UseAbilityDirection = base.Executor.GetLookAtAngle(vector);
				}
			}
			else
			{
				vector.y = 0f;
				if (base.Executor.MaybeAnimationManager != null)
				{
					base.Executor.MaybeAnimationManager.UseAbilityDirection = ((vector == Vector3.zero) ? 0f : Quaternion.LookRotation(vector).eulerAngles.y);
				}
			}
		}
		if (Ability.NeedLoS && base.Executor.GetOptional<UnitPartJump>()?.Active == null)
		{
			CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)(GraphNode)base.Executor.CurrentNode;
			CustomGridNodeBase bestShootingPosition = Ability.GetBestShootingPosition(customGridNodeBase, base.Target);
			if ((object)base.Executor.MaybeAnimationManager != null)
			{
				int num = (bestShootingPosition.XCoordinateInGrid - customGridNodeBase.XCoordinateInGrid) * (base.Target.NearestNode.ZCoordinateInGrid - customGridNodeBase.ZCoordinateInGrid) - (bestShootingPosition.ZCoordinateInGrid - customGridNodeBase.ZCoordinateInGrid) * (base.Target.NearestNode.XCoordinateInGrid - customGridNodeBase.XCoordinateInGrid);
				UnitAnimationManager maybeAnimationManager = base.Executor.MaybeAnimationManager;
				UnitAnimationActionCover.StepOutDirectionAnimationType stepOutDirectionAnimationType = ((num > 0) ? UnitAnimationActionCover.StepOutDirectionAnimationType.Right : ((num < 0) ? UnitAnimationActionCover.StepOutDirectionAnimationType.Left : UnitAnimationActionCover.StepOutDirectionAnimationType.None));
				maybeAnimationManager.StepOutDirectionAnimationType = stepOutDirectionAnimationType;
				base.Executor.MaybeAnimationManager.AbilityIsSpell = Ability.Blueprint.IsSpell;
			}
		}
		bool isCornerAttack = false;
		ItemEntityWeapon weapon = Ability.Weapon;
		if (weapon != null && weapon.Blueprint.IsMelee)
		{
			Int3 position = base.Executor.CurrentNode.node.position;
			Int3? @int = base.Target?.NearestNode.position;
			if (@int.HasValue)
			{
				Int3 value = position;
				Int3? int2 = @int;
				Int3? int3 = value - int2;
				isCornerAttack = (int3.Value.x != 0 && int3.Value.z != 0) || (int3.Value.x == 0 && int3.Value.z == 0);
			}
		}
		IAbilityCustomAnimation abilityCustomAnimation = base.Params.CustomAnimationOverride ?? Ability.Blueprint.GetComponent<IAbilityCustomAnimation>();
		if (abilityCustomAnimation != null)
		{
			UnitAnimationAction unitAnimationAction = abilityCustomAnimation.GetAbilityAction(base.Executor)?.Load();
			if (!unitAnimationAction)
			{
				ScheduleAct();
			}
			else if (base.Executor.View.AnimationManager?.CreateHandle(unitAnimationAction) is UnitAnimationActionHandle unitAnimationActionHandle)
			{
				if (unitAnimationActionHandle.Action is UnitAnimationActionSimple unitAnimationActionSimple && abilityCustomAnimation is AbilityCustomAnimationByBuff { OverrideAnimationType: not UnitAnimationType.Unused } abilityCustomAnimationByBuff)
				{
					unitAnimationActionSimple.OverrideUnitAnimationType(abilityCustomAnimationByBuff.OverrideAnimationType);
				}
				SetHandlesParameters(unitAnimationActionHandle);
				unitAnimationActionHandle.IsCornerAttack = isCornerAttack;
				StartAnimation(unitAnimationActionHandle);
			}
			else
			{
				PFLog.Default.Error(base.Executor.View, $"{base.Executor} cannot start custom animation {unitAnimationAction} for {Ability.Blueprint}");
				ScheduleAct();
			}
		}
		else
		{
			UnitAnimationActionLink unitAnimationActionLink = Ability.FXSettings?.GetAnimation(IsMainHandAttack(Ability), isCornerAttack);
			UnitPartFXSettingOverride optional = base.Executor.GetOptional<UnitPartFXSettingOverride>();
			if (optional != null && unitAnimationActionLink != null && optional.ActionsOverride.TryGetValue(unitAnimationActionLink, out var value2))
			{
				unitAnimationActionLink = value2;
			}
			if ((object)unitAnimationActionLink != null && unitAnimationActionLink.Exists())
			{
				StartPatternAnimation(unitAnimationActionLink.Load());
			}
			else if (base.Executor.IsInSquad && !Ability.Blueprint.IsSpell && IsMainHandAttack(Ability) && Ability.Weapon != null && !Ability.Weapon.Blueprint.IsMelee && base.Executor.View != null && base.Executor.View.AnimationManager != null && base.Executor.View.AnimationManager.CurrentMainHandAttackForPrepare != null)
			{
				base.Animation = base.Executor.View.AnimationManager.CurrentMainHandAttackForPrepare;
				base.HasAnimation = true;
				if (base.Animation == null)
				{
					ScheduleAct();
					return;
				}
				base.Animation.AttackWeaponStyle = WeaponStyle;
				base.Animation.AlternativeStyle = Ability.Blueprint.GetComponent<WarhammerAttackAlternativeAnimationStyle>()?.WeaponAnimationStyle ?? AnimationAlternativeStyle.None;
				base.Animation.IsBurst = Ability.IsBurstAttack;
				base.Animation.BurstCount = Ability.BurstAttacksCount;
				base.Animation.BurstAnimationDelay = Ability.Weapon?.Blueprint.VisualParameters.BurstAnimationDelay ?? 0f;
				base.Animation.IsPreparingForShooting = false;
				base.Animation.StartInternal();
			}
			else
			{
				if (base.Executor.View?.AnimationManager?.CurrentMainHandAttackForPrepare != null)
				{
					base.Executor.View.AnimationManager.CurrentMainHandAttackForPrepare.Release();
				}
				StartAnimation((!Ability.Blueprint.IsSpell) ? (IsMainHandAttack(Ability) ? UnitAnimationType.MainHandAttack : UnitAnimationType.OffHandAttack) : ((Ability.Blueprint.Animation == UnitAnimationActionCastSpell.CastAnimationStyle.Reload) ? UnitAnimationType.Reload : UnitAnimationType.CastSpell), UnitAnimationActionHandleInitializer);
			}
		}
		base.TriggerAnimation();
		void StartPatternAnimation(UnitAnimationAction action)
		{
			if (((base.Executor.View.AnimationManager != null) ? base.Executor.View.AnimationManager.CreateHandle(action) : null) is UnitAnimationActionHandle unitAnimationActionHandle2)
			{
				SetHandlesParameters(unitAnimationActionHandle2);
				unitAnimationActionHandle2.IsCornerAttack = isCornerAttack;
				StartAnimation(unitAnimationActionHandle2);
			}
		}
		void UnitAnimationActionHandleInitializer(UnitAnimationActionHandle h)
		{
			SetHandlesParameters(h);
			h.CastStyle = m_CastAnimStyle;
			h.CastInOffhand = Ability.Blueprint.CastInOffHand;
			if (Ability.Blueprint.IsSpell && Ability.Blueprint.Animation == UnitAnimationActionCastSpell.CastAnimationStyle.Reload && !IsMainHandAttack(Ability))
			{
				h.CastInOffhand = true;
			}
			h.IsCornerAttack = isCornerAttack;
			h.BurstAnimationDelay = Ability.Weapon?.Blueprint.VisualParameters.BurstAnimationDelay ?? 0f;
			h.IsBladeDance = HasTwoMeleeForBladeDance();
		}
	}

	private void TryStopBuffLoopAction()
	{
		base.Executor.Facts.GetAll((EntityFact ef) => ef.GetComponent<PlayLoopAnimationByBuff>() != null).ForEach(delegate(EntityFact ef)
		{
			ef.CallComponents(delegate(PlayLoopAnimationByBuff playLoop)
			{
				playLoop.TryResetAction();
			});
		});
	}

	private void SetHandlesParameters(UnitAnimationActionHandle h)
	{
		h.CastingTime = m_CastTime;
		h.Spell = Ability.Blueprint;
		h.SpecialCastBehaviour = m_Special;
		h.AttackWeaponStyle = WeaponStyle;
		h.AlternativeStyle = Ability.Blueprint.GetComponent<WarhammerAttackAlternativeAnimationStyle>()?.WeaponAnimationStyle ?? AnimationAlternativeStyle.None;
		h.AttackTargetDistance = (base.Target.Point - base.Executor.Position).magnitude;
		h.IsBurst = Ability.IsBurstAttack;
		h.BurstCount = Ability.BurstAttacksCount;
		h.Recoil = (Ability.FXSettings?.VisualFXSettings?.Recoil).GetValueOrDefault();
	}

	private bool HasTwoMeleeForBladeDance()
	{
		AbilityCustomBladeDance component = Ability.Blueprint.GetComponent<AbilityCustomBladeDance>();
		if (component == null)
		{
			return false;
		}
		ItemEntityWeapon maybeWeapon = base.Executor.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon maybeWeapon2 = base.Executor.Body.SecondaryHand.MaybeWeapon;
		if (!component.UseSpecificWeaponClassification)
		{
			if (!component.UseSpecificWeapon && maybeWeapon != null)
			{
				return maybeWeapon2 != null;
			}
			return false;
		}
		if (!component.UseSpecificWeapon && maybeWeapon?.Blueprint?.Classification == component.Classification)
		{
			return maybeWeapon2?.Blueprint?.Classification == component.Classification;
		}
		return false;
	}

	protected override void StartAnimation(UnitAnimationActionHandle handle)
	{
		ItemEntityWeapon weapon = Ability.Weapon;
		MechadendritesType mechadendritesType = ((weapon == null || weapon.Blueprint.IsMelee) ? Ability.Blueprint.UsedMechadendrite : MechadendritesType.Ballistic);
		ItemEntityWeapon weapon2 = Ability.Weapon;
		if (((weapon2 != null && !weapon2.Blueprint.IsMelee) || Ability.Blueprint.UseOnMechadendrite) && base.Executor.GetOptional<UnitPartMechadendrites>() != null)
		{
			UnitPartMechadendrites optional = base.Executor.GetOptional<UnitPartMechadendrites>();
			if (optional != null && optional.Mechadendrites.ContainsKey(mechadendritesType))
			{
				base.Animation = handle;
				base.HasAnimation = true;
				UnitAnimationActionHandle unitAnimationActionHandle = (UnitAnimationActionHandle)handle.Clone();
				handle.AttackWeaponStyle = WeaponAnimationStyle.Mechadendrites;
				handle.CastStyle = UnitAnimationActionCastSpell.CastAnimationStyle.Mechadendrites;
				unitAnimationActionHandle.ChangeManager(base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites[mechadendritesType]?.AnimationManager);
				if (unitAnimationActionHandle.Action != null)
				{
					base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites[mechadendritesType]?.AnimationManager.Execute(unitAnimationActionHandle);
					Dictionary<MechadendritesType, MechadendriteSettings>.ValueCollection valueCollection = base.Executor.GetOptional<UnitPartMechadendrites>()?.Mechadendrites.Values;
					if (valueCollection != null && !valueCollection.Empty())
					{
						foreach (MechadendriteSettings item in valueCollection)
						{
							if (item.MechadendritesType != mechadendritesType)
							{
								UnitAnimationActionHandle unitAnimationActionHandle2 = (UnitAnimationActionHandle)handle.Clone();
								unitAnimationActionHandle2.AttackWeaponStyle = WeaponAnimationStyle.Mechadendrites;
								unitAnimationActionHandle2.CastStyle = UnitAnimationActionCastSpell.CastAnimationStyle.Mechadendrites;
								unitAnimationActionHandle2.ChangeManager(item.AnimationManager);
								if (unitAnimationActionHandle2.Action != null)
								{
									item.AnimationManager.Execute(unitAnimationActionHandle2);
								}
							}
						}
					}
				}
			}
		}
		if (base.Params.OverrideAnimationHandle is UnitAnimationActionHandle animation)
		{
			base.Animation = animation;
			base.HasAnimation = true;
		}
		else
		{
			base.StartAnimation(handle);
		}
	}

	private bool IsMainHandAttack(AbilityData ability)
	{
		PartUnitBody bodyOptional = ability.Caster.GetBodyOptional();
		if (bodyOptional == null)
		{
			return true;
		}
		if (ability.OverrideWeapon != null)
		{
			if (ability.Weapon == bodyOptional.SecondaryHand?.MaybeWeapon)
			{
				return false;
			}
			return true;
		}
		if (ability.Caster?.GetOptional<UnitPartMechadendrites>() == null)
		{
			return ability.Weapon == bodyOptional.PrimaryHand?.MaybeWeapon;
		}
		return true;
	}

	protected override Vector3 GetTargetPoint()
	{
		return base.Target.Point;
	}

	public override void TurnToTarget()
	{
		if (SyncRotationAndAttack)
		{
			m_RotateIsDone = false;
		}
		BlueprintAbilityFXSettings fXSettings = Ability.FXSettings;
		if (fXSettings != null && fXSettings.ShouldOffsetTargetRelativePosition)
		{
			Vector3 vector = Quaternion.LookRotation((base.ApproachPoint - base.Executor.Position).normalized) * Ability.FXSettings.OffsetTargetPosition;
			Vector3 point = base.ApproachPoint + vector;
			base.Executor.LookAt(point);
		}
		else
		{
			base.TurnToTarget();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		Ability.IgnoreUsingInThreateningArea = IgnoreAbilityUsingInThreateningArea;
		if (m_Special != UnitAnimationActionCastSpell.SpecialBehaviourType.NoPrecast)
		{
			CastAbilityFx(AbilitySpawnFxTime.OnPrecastStart);
		}
		EventBus.RaiseEvent(delegate(IVisualWeaponStateChangeHandle h)
		{
			h.VisualWeaponStateChangeHandle(VFXSpeedUpdater.WeaponVisualState.InAttack, (!(base.Executor?.View != null)) ? null : base.Executor?.View.HandsEquipment?.GetWeaponModel(offHand: false));
		});
	}

	[NotNull]
	private List<GameObject> SureGroundFxList()
	{
		return m_GroundFxObjects ?? (m_GroundFxObjects = new List<GameObject>());
	}

	private void CastAbilityFx(AbilitySpawnFxTime time)
	{
		BlueprintComponentsEnumerator<AbilitySpawnFx> components = Ability.Blueprint.GetComponents<AbilitySpawnFx>();
		AbilityExecutionContext context = new AbilityExecutionContext(Ability, base.Target, Ability.Caster.Position);
		foreach (AbilitySpawnFx item in components)
		{
			if (item.Time == time)
			{
				item.Spawn(context, null, item.DestroyOnCast ? SureGroundFxList() : null);
			}
		}
		if (time == AbilitySpawnFxTime.OnPrecastStart)
		{
			SpawnPrecastFx();
		}
	}

	private void SpawnPrecastFx()
	{
		SpellSource spellSource = Ability.SpellSource;
		switch (spellSource)
		{
		case SpellSource.None:
			return;
		case SpellSource.Unknown:
			spellSource = SpellSource.Arcane;
			break;
		}
		if (Ability.SpellLevel > 0)
		{
			string text = base.Executor.Gender.ToString();
			string text2 = Ability.Blueprint.School.ToString();
			AbilityData.VoiceIntensityType voiceIntensity = Ability.VoiceIntensity;
			if (voiceIntensity != 0 && !base.Executor.SilentCaster)
			{
				SoundEventsManager.PostEvent(spellSource.ToString() + "_" + text2 + "_" + voiceIntensity.ToString() + "_" + text, base.Executor.View.gameObject);
			}
		}
	}

	protected override void OnTick()
	{
		base.OnTick();
		if (SyncRotationAndAttack)
		{
			if (Mathf.Abs(Mathf.DeltaAngle(base.Executor.Orientation, base.Executor.DesiredOrientation)) < 5f)
			{
				m_RotateIsDone = true;
				float num = Ability.Blueprint.SyncRotationDelay;
				if (num < 0f)
				{
					num = 0f;
				}
				Game.Instance.CustomCallbackController.InvokeInTime(delegate
				{
					ForceImmediateAction();
				}, num);
			}
			else
			{
				m_RotateIsDone = false;
			}
		}
		if (base.Animation != null && base.Animation.IsPrecastFinished && !m_AfterPrecastFxSpawned)
		{
			m_AfterPrecastFxSpawned = true;
			CastAbilityFx(AbilitySpawnFxTime.OnPrecastFinished);
		}
		AbilityExecutionProcess executionProcess = ExecutionProcess;
		if (executionProcess != null && executionProcess.IsEnded)
		{
			UnitAnimationActionHandle animation = base.Animation;
			if (animation == null || animation.IsReleased)
			{
				ForceFinish(ResultType.Success);
			}
		}
		if (!base.IsActed && !Ability.CanTarget(base.Target) && !base.FromCutscene)
		{
			Interrupt();
		}
	}

	protected override ResultType OnAction()
	{
		if (SyncRotationAndAttack && !RotateIsDone)
		{
			return ResultType.None;
		}
		if (CurrentActionIndex > 0)
		{
			if (CurrentActionIndex >= ActionsCount)
			{
				PFLog.Default.Error($"CurrentActionIndex {CurrentActionIndex} >= ActionsCount {ActionsCount}");
				return ResultType.Fail;
			}
			if (ExecutionProcess == null)
			{
				PFLog.Default.Error("ExecutionProcess == null");
				return ResultType.Fail;
			}
			ExecutionProcess.Context.NextAction();
			if (!ExecutionProcess.IsEngageUnit && CurrentActionIndex >= ActionsCount)
			{
				return ResultType.Success;
			}
			return ResultType.None;
		}
		using (ContextData<AbilityData.IgnoreCooldown>.RequestIf(IgnoreCooldown))
		{
			using (ContextData<AbilityData.ForceFreeAction>.RequestIf(base.IsFreeAction))
			{
				if (!ForceCastOnBadTarget && !Ability.IsAvailable)
				{
					return ResultType.Fail;
				}
			}
		}
		if (IsTargetingDeadUnit)
		{
			return ResultType.Fail;
		}
		if (!ForceCastOnBadTarget)
		{
			MechanicEntity entity = base.Target.Entity;
			if (entity != null && !entity.IsInGame)
			{
				return ResultType.Fail;
			}
		}
		if (!base.FromCutscene && !Ability.CanTarget(base.Target))
		{
			return ResultType.Fail;
		}
		if (!base.FromCutscene)
		{
			ItemEntity sourceItem = Ability.SourceItem;
			if (sourceItem != null && sourceItem.IsSpendCharges && sourceItem.Charges <= 0)
			{
				return ResultType.Fail;
			}
		}
		bool isBonusUsage = Ability.IsBonusUsage;
		int num = Ability.CalculateActionPointCost();
		RulePerformAbility rulePerformAbility = new RulePerformAbility(Ability, base.Target);
		rulePerformAbility.IsCutscene = base.FromCutscene;
		rulePerformAbility.Context.DisableLog = DisableLog;
		rulePerformAbility.Context.HitPolicy = HitPolicy;
		rulePerformAbility.Context.DamagePolicy = DamagePolicy;
		rulePerformAbility.Context.KillTarget = KillTarget;
		rulePerformAbility.Context.AllTargets = base.Params.AllTargets;
		rulePerformAbility.DisableGameLog = DisableLog;
		rulePerformAbility.IgnoreCooldown = IgnoreCooldown;
		rulePerformAbility.ForceFreeAction = base.IsFreeAction;
		RulePerformAbility rulePerformAbility2 = rulePerformAbility;
		rulePerformAbility2.Context.SetSourceAbility(base.Params.OriginatedFrom);
		Rulebook.Trigger(rulePerformAbility2);
		ExecutionProcess = rulePerformAbility2.Result;
		if (ExecutionProcess == null)
		{
			return ResultType.Fail;
		}
		if (IsInstantDeliver)
		{
			ExecutionProcess.InstantDeliver();
		}
		if (!base.FromCutscene)
		{
			RuleCalculateNotSpendItemChance ruleCalculateNotSpendItemChance = new RuleCalculateNotSpendItemChance(Ability.Caster, Ability);
			Rulebook.Trigger(ruleCalculateNotSpendItemChance);
			if (!ruleCalculateNotSpendItemChance.Success)
			{
				Ability.Spend();
			}
		}
		if (!rulePerformAbility2.Success)
		{
			return ResultType.Fail;
		}
		if (m_HandFxObjects != null)
		{
			foreach (GameObject handFxObject in m_HandFxObjects)
			{
				FxHelper.Destroy(handFxObject);
			}
			m_HandFxObjects = null;
		}
		if (m_GroundFxObjects != null)
		{
			foreach (GameObject groundFxObject in m_GroundFxObjects)
			{
				FxHelper.Destroy(groundFxObject);
			}
			m_GroundFxObjects = null;
		}
		if (!base.IsFreeAction && num > 0)
		{
			base.Executor.CombatState.SpendActionPoints(num);
			EventBus.RaiseEvent(delegate(IUnitActionPointsHandler h)
			{
				h.HandleActionPointsSpent(base.Executor);
			});
		}
		if (!base.FromCutscene && !IgnoreCooldown && !isBonusUsage)
		{
			base.Executor.GetAbilityCooldownsOptional()?.StartCooldown(Ability);
		}
		ExecutionProcess.Context.NextAction();
		if (!ExecutionProcess.IsEngageUnit && CurrentActionIndex >= ActionsCount)
		{
			return ResultType.Success;
		}
		return ResultType.None;
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		Ability.IgnoreUsingInThreateningArea = false;
		EventBus.RaiseEvent(delegate(IVisualWeaponStateChangeHandle h)
		{
			h.VisualWeaponStateChangeHandle(VFXSpeedUpdater.WeaponVisualState.InHand, (!(base.Executor?.View != null)) ? null : base.Executor?.View.HandsEquipment?.GetWeaponModel(offHand: false));
		});
		if (base.Executor?.View != null)
		{
			base.Executor.View.HideOffWeapon(hide: false);
		}
		if ((m_Special != UnitAnimationActionCastSpell.SpecialBehaviourType.NoCast || base.Result != ResultType.Success) && m_HandFxObjects != null)
		{
			foreach (GameObject handFxObject in m_HandFxObjects)
			{
				FxHelper.Destroy(handFxObject);
			}
			m_HandFxObjects = null;
		}
		if (m_GroundFxObjects != null)
		{
			foreach (GameObject groundFxObject in m_GroundFxObjects)
			{
				FxHelper.Destroy(groundFxObject);
			}
			m_GroundFxObjects = null;
		}
		if (base.Result == ResultType.Interrupt)
		{
			ExecutionProcess?.Dispose();
		}
	}

	protected override string GetInnerDataDescription()
	{
		return Ability.Blueprint.NameSafe();
	}
}
