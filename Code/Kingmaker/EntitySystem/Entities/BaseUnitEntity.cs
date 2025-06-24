using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Designers.Mechanics.Facts;
using Kingmaker.Code.UnitLogic;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Optimization;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Kingmaker.Visual.HitSystem;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.EntitySystem.Entities;

public abstract class BaseUnitEntity : AbstractUnitEntity, PartUnitAlignment.IOwner, IEntityPartOwner<PartUnitAlignment>, IEntityPartOwner, PartUnitCombatState.IOwner, IEntityPartOwner<PartUnitCombatState>, PartFaction.IOwner, IEntityPartOwner<PartFaction>, PartCombatGroup.IOwner, IEntityPartOwner<PartCombatGroup>, PartVision.IOwner, IEntityPartOwner<PartVision>, PartUnitStealth.IOwner, IEntityPartOwner<PartUnitStealth>, PartUnitProgression.IOwner, IEntityPartOwner<PartUnitProgression>, PartStatsAttributes.IOwner, IEntityPartOwner<PartStatsAttributes>, PartStatsSkills.IOwner, IEntityPartOwner<PartStatsSkills>, PartStatsSaves.IOwner, IEntityPartOwner<PartStatsSaves>, PartUnitProficiency.IOwner, IEntityPartOwner<PartUnitProficiency>, PartAbilityResourceCollection.IOwner, IEntityPartOwner<PartAbilityResourceCollection>, PartInventory.IOwner, IEntityPartOwner<PartInventory>, PartUnitBody.IOwner, IEntityPartOwner<PartUnitBody>, PartUnitBrain.IOwner, IEntityPartOwner<PartUnitBrain>, PartUnitDescription.IOwner, IEntityPartOwner<PartUnitDescription>, PartAbilityCooldowns.IOwner, IEntityPartOwner<PartAbilityCooldowns>, ILootable, IBaseUnitEntity, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable, IFakeSelectHandler<EntitySubscriber>, IFakeSelectHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntitySubscriber, IEventTag<IFakeSelectHandler, EntitySubscriber>, IHashable
{
	public new interface IUnitAsleepHandler<TTag> : IUnitAsleepHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IUnitAsleepHandler, TTag>
	{
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	private List<BlueprintUnitUpgrader> m_AppliedUpgraders;

	[JsonProperty(PropertyName = "IsExtra")]
	[GameStateIgnore]
	private bool m_IsExtra;

	private MechanicsContext m_CachedContext;

	private bool m_IsCloneOfManeCharacter;

	public readonly CountableFlag PreventDirectControl = new CountableFlag();

	private bool m_LootViewed;

	[JsonProperty]
	[GameStateIgnore]
	public bool IsSelected { get; set; } = true;


	[JsonProperty]
	[GameStateIgnore]
	public bool IsFakeSelected { get; set; }

	[JsonProperty]
	[GameStateIgnore]
	public bool IsLink { get; set; } = true;


	[JsonProperty]
	public float TimeToNextRoundTick { get; set; }

	[JsonProperty]
	public int CachedPerceptionRoll { get; set; }

	[JsonProperty]
	public bool GiveExperienceOnDeath { get; set; } = true;


	[JsonProperty]
	public TimeSpan? LastRestTime { get; set; }

	[JsonProperty]
	public string MusicBossFightTypeGroup { get; set; }

	[JsonProperty]
	public string MusicBossFightTypeValue { get; set; }

	public override bool IsExtra => m_IsExtra;

	[JsonProperty]
	[GameStateIgnore]
	public bool SpawnFromPsychicPhenomena { get; private set; }

	[JsonProperty(PropertyName = "MasterRef")]
	private EntityRef<BaseUnitEntity> m_MasterRef { get; set; }

	public ReactiveCommand UpdateCommand { get; } = new ReactiveCommand();


	public EntityRef<BaseUnitEntity> CopyOf { get; set; }

	public AbilityCollection Abilities { get; private set; }

	public ActivatableAbilityCollection ActivatableAbilities { get; private set; }

	public override bool LootViewed => m_LootViewed;

	public new MainUnitFact MainFact => (MainUnitFact)base.MainFact;

	public PortraitData Portrait => UISettings.Portrait;

	public override bool IsInLockControlCutscene => CutsceneControlledUnit.GetControllingPlayer(this)?.Cutscene.LockControl ?? false;

	public override bool AddToGrid => true;

	public new UnitEntityView View => (UnitEntityView)base.View;

	public bool AiMovementForbidden
	{
		get
		{
			if (!IsDirectlyControllable || !base.HoldState)
			{
				return !State.CanMove;
			}
			return true;
		}
	}

	public override bool IsDeadAndHasLoot
	{
		get
		{
			if (base.LifeState.IsFinallyDead)
			{
				return Inventory.HasLoot;
			}
			return false;
		}
	}

	public bool IsMainCharacter
	{
		get
		{
			if (GetOptional<UnitPartMainCharacter>() == null)
			{
				if (CopyOf.Entity != null)
				{
					return CopyOf.Entity.IsMainCharacter;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsCloneOfMainCharacter
	{
		get
		{
			if (m_IsCloneOfManeCharacter)
			{
				return true;
			}
			return Game.Instance.Player.MainCharacterEntity?.Blueprint == base.Blueprint;
		}
	}

	public override bool IsDirectlyControllable
	{
		get
		{
			if ((bool)PreventDirectControl)
			{
				return false;
			}
			AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
			if (loadedAreaState != null && loadedAreaState.Settings.CapitalPartyMode)
			{
				return this == Game.Instance.Player.MainCharacterEntity;
			}
			if (!Faction.IsDirectlyControllable || base.LifeState.IsFinallyDead || State.IsPanicked || IsDetached)
			{
				return false;
			}
			if (GetOptional<UnitPartSummonedMonster>() != null)
			{
				return Faction.IsDirectlyControllable;
			}
			if (this.HasMechanicFeature(MechanicsFeatureType.ForceAIControl))
			{
				return false;
			}
			UnitPartCompanion unitPartCompanion = Master?.GetOptional<UnitPartCompanion>() ?? GetOptional<UnitPartCompanion>();
			if (unitPartCompanion != null && unitPartCompanion.State != CompanionState.ExCompanion)
			{
				return unitPartCompanion.State != CompanionState.Remote;
			}
			return false;
		}
	}

	public override bool AreHandsBusyWithAnimation
	{
		get
		{
			CountingGuard countingGuard = ObjectExtensions.Or(View, null)?.HandsEquipment?.AreHandsBusyWithAnimation;
			if (countingGuard == null)
			{
				return false;
			}
			return countingGuard;
		}
	}

	public BloodType BloodType => base.Blueprint.VisualSettings.BloodType;

	public bool IsDetached
	{
		get
		{
			BaseUnitEntity master = Master;
			if (master?.View != null)
			{
				return master.IsDetached;
			}
			return GetCompanionState() == CompanionState.InPartyDetached;
		}
	}

	public override float Corpulence
	{
		get
		{
			if (!View)
			{
				return base.Corpulence;
			}
			return View.Corpulence;
		}
	}

	public bool SilentCaster => GetOptional<PartPolymorphed>()?.Component?.SilentCaster ?? base.Blueprint.VisualSettings.SilentCaster;

	public BaseUnitEntity Master => m_MasterRef;

	public bool IsPet => Master != null;

	public bool IsMaster => GetOptional<UnitPartPetOwner>()?.HasPet ?? false;

	public BaseUnitEntity Pet => GetOptional<UnitPartPetOwner>()?.PetUnit;

	public int BaseCR => Math.Max(0, CR);

	public int CR => base.Blueprint.CR;

	public IEnumerable<Spellbook> Spellbooks => Enumerable.Empty<Spellbook>();

	public override ViewHandlingOnDisposePolicyType DefaultViewHandlingOnDisposePolicy => ViewHandlingOnDisposePolicyType.Destroy;

	public override bool IsSuppressible => true;

	public override bool IsAffectedByFogOfWar => true;

	public override bool AlwaysRevealedInFogOfWar => IsDirectlyControllable;

	public override Type RequiredBlueprintType => typeof(BlueprintUnitFact);

	[NotNull]
	public MechanicsContext Context
	{
		get
		{
			MechanicsContext mechanicsContext = m_CachedContext;
			if (mechanicsContext == null)
			{
				MechanicsContext obj = MainFact.MaybeContext ?? new MechanicsContext(this, this, base.OriginalBlueprint, null, this);
				MechanicsContext mechanicsContext2 = obj;
				m_CachedContext = obj;
				mechanicsContext = mechanicsContext2;
			}
			return mechanicsContext;
		}
	}

	public override Size OriginalSize
	{
		get
		{
			PartUnitProgression progression = Progression;
			return ((progression == null) ? null : SimpleBlueprintExtendAsObject.Or(progression.Race, null)?.Size) ?? base.Blueprint.Size;
		}
	}

	public bool HasUMDSkill => Skills.SkillLoreXenos.BaseValue > 0;

	public bool IsEssentialForGame
	{
		get
		{
			if (!GetOptional<UnitPartMainCharacter>())
			{
				return GetOptional<UnitPartEssential>();
			}
			return true;
		}
	}

	public BlueprintUnit BlueprintForInspection => GetOptional<PartPolymorphed>()?.ReplaceBlueprintForInspection ?? base.Blueprint;

	public bool IsInvisible => GetOptional<PartUnitInvisible>();

	public override bool IsCheater
	{
		get
		{
			if (!base.IsCheater)
			{
				return base.Blueprint.IsCheater;
			}
			return true;
		}
	}

	public override UnitMovementAgentBase MaybeMovementAgent => ObjectExtensions.Or(View, null)?.MovementAgent;

	public override bool CanBeAttackedDirectly => true;

	public override bool IsPreview => GetOptional<PartPreviewUnit>() != null;

	public bool HasAssassinCareer
	{
		get
		{
			try
			{
				string guid = BlueprintRoot.Instance.AssassinCareerPathGuid;
				return guid != null && Facts?.Get((Feature feature) => feature.Blueprint.AssetGuid == guid) != null;
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				return false;
			}
		}
	}

	public abstract PartUnitAlignment Alignment { get; }

	public abstract PartUnitCombatState CombatState { get; }

	public abstract PartFaction Faction { get; }

	public abstract PartCombatGroup CombatGroup { get; }

	public abstract PartVision Vision { get; }

	public abstract PartUnitStealth Stealth { get; }

	public abstract PartUnitProgression Progression { get; }

	public abstract PartStatsAttributes Attributes { get; }

	public abstract PartStatsSkills Skills { get; }

	public abstract PartStatsSaves Saves { get; }

	public abstract PartUnitProficiency Proficiencies { get; }

	public abstract PartAbilityResourceCollection AbilityResources { get; }

	public abstract PartInventory Inventory { get; }

	public abstract PartUnitDescription Description { get; }

	public abstract PartUnitBrain Brain { get; }

	public abstract PartUnitBody Body { get; }

	public PartAbilityCooldowns AbilityCooldowns => GetRequired<PartAbilityCooldowns>();

	public PartUnitUISettings UISettings => GetOrCreate<PartUnitUISettings>();

	[CanBeNull]
	public UnitPartEncumbrance EncumbranceData => GetOptional<UnitPartEncumbrance>();

	public override PartSavedRagdollState SavedRagdoll => GetOptional<PartSavedRagdollState>();

	public override SavedDismembermentState SavedDismemberment => GetOptional<SavedDismembermentState>();

	string ILootable.Name => base.CharacterName;

	string ILootable.Description => null;

	public BaseUnitEntity OwnerEntity => this;

	public ItemsCollection Items => Inventory.Collection;

	public List<BlueprintCargoReference> Cargo => null;

	public Func<ItemEntity, bool> CanInsertItem => null;

	public bool MeetsPrerequisite(PrerequisiteStat stat)
	{
		return GetStatOptional(stat.Stat)?.PermanentValue >= stat.MinValue;
	}

	public void MarkLootViewed()
	{
		m_LootViewed = true;
	}

	public LosCalculations.CoverType GetCoverType()
	{
		return LosCalculations.GetCoverType((CustomGridNode)base.CurrentNode.node);
	}

	protected BaseUnitEntity(string uniqueId, bool isInGame, BlueprintUnit blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected BaseUnitEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public void UpdateVisible()
	{
		View.SetVisible(!IsInvisible || CombatGroup.IsPlayerParty, force: true);
	}

	public override float GetWarhammerMovementApPerCellThreateningArea()
	{
		return PartWarhammerMovementApPerCellThreateningArea.GetThreateningArea(this);
	}

	protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return new MainUnitFact((BlueprintUnit)blueprint);
	}

	public override StatBaseValue GetStatBaseValue(StatType type)
	{
		return StatsHelper.GetUnitStatBaseValue(type, base.Blueprint);
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		Abilities = Facts.EnsureFactProcessor<AbilityCollection>();
		Abilities.SetSubscribedOnEventBus(base.IsInGame);
		ActivatableAbilities = Facts.EnsureFactProcessor<ActivatableAbilityCollection>();
		ActivatableAbilities.SetSubscribedOnEventBus(base.IsInGame);
	}

	protected override void OnApplyPostLoadFixes()
	{
		base.OnApplyPostLoadFixes();
		Inventory?.ApplyPostLoadFixes();
		Abilities.SetActiveForce(active: true);
		ActivatableAbilities.SetActiveForce(active: true);
		UnitUpgraderHelper.ApplyUpgraders(this, base.OriginalBlueprint, fromPlaceholder: false, ref m_AppliedUpgraders);
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartStatsContainer>();
		GetOrCreate<PartAbilityCooldowns>();
		GetOrCreate<EntityBoundsPart>();
		GetOrCreate<PartAbilitySettings>();
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			GetOrCreate<PartPreviewUnit>();
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		GetOrCreate<LevelUpPlanUnitHolder>();
		if (!Faction.IsPlayer && !this.IsStarship())
		{
			AddFact(BlueprintRoot.Instance.WarhammerRoot.CommonMobFact);
			foreach (BlueprintUnitFact difficultyFact in BlueprintWarhammerRoot.Instance.DifficultyRoot.GetDifficultyFacts(base.OriginalBlueprint.DifficultyType))
			{
				AddFact(difficultyFact);
			}
		}
		using (ProfileScope.New("Add Facts from army"))
		{
			BlueprintArmyDescription army = base.OriginalBlueprint.Army;
			if (army != null)
			{
				foreach (BlueprintFeature feature in army.Features)
				{
					if (feature != null)
					{
						DoNotAddFeatureFromArmy component = base.Blueprint.GetComponent<DoNotAddFeatureFromArmy>();
						if (component == null || !component.Features.Contains(feature))
						{
							AddFact(feature);
						}
					}
				}
			}
		}
		using (ProfileScope.New("Add Facts"))
		{
			foreach (BlueprintUnitFact item in base.OriginalBlueprint.AddFacts.EmptyIfNull().NotNull())
			{
				AddFact(item).AddSource(base.OriginalBlueprint);
			}
		}
		if (!ContextData<UnitHelper.ChargenUnit>.Current)
		{
			using (ProfileScope.New("Starting Inventory"))
			{
				AddStartingInventory();
			}
		}
		Remove<LevelUpPlanUnitHolder>();
		UnitUpgraderHelper.SetAllUpgradersApplied(base.OriginalBlueprint, fromPlaceholder: false, ref m_AppliedUpgraders);
	}

	public void AddStartingInventory()
	{
		if ((bool)ContextData<UnitHelper.DoNotCreateItems>.Current)
		{
			return;
		}
		using (ProfileScope.New("Add Item"))
		{
			using (ContextData<ItemsCollection.SuppressEvents>.Request())
			{
				foreach (BlueprintItem item in base.OriginalBlueprint.StartingInventory.NotNull())
				{
					Inventory.Add(item);
				}
			}
		}
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		if ((bool)Parts.GetOptional<UnitPartCompanion>() || IsPet)
		{
			Game.Instance.Player.InvalidateCharacterLists();
		}
		ActivatableAbilities.SetSubscribedOnEventBus(base.IsInGame);
		Abilities.SetSubscribedOnEventBus(base.IsInGame);
		if (!base.IsInGame && IsInCombat)
		{
			CombatState.LeaveCombat();
		}
	}

	protected override void OnDestroy()
	{
		if (Inventory.IsLootDroppedAsEntity)
		{
			Inventory.TransferInventoryToDroppedLoot();
		}
		base.OnDestroy();
	}

	protected override void OnDispose()
	{
		if (IsInCombat)
		{
			CombatState.LeaveCombat();
		}
		base.OnDispose();
	}

	public UnitEntityView CreateView()
	{
		UnitEntityView unitEntityView = ViewSettings.Instantiate();
		if (unitEntityView != null)
		{
			unitEntityView.Blueprint = base.Blueprint;
		}
		return unitEntityView;
	}

	protected override IEntityViewBase CreateViewForData()
	{
		try
		{
			return CreateView();
		}
		catch (Exception exception)
		{
			if (base.Blueprint != null)
			{
				PFLog.Default.ExceptionWithReport(exception, "Fail create view for " + base.Blueprint.name + ".");
			}
			else
			{
				PFLog.Default.ExceptionWithReport(exception, "Fail create view for [???] (UnitEntityData not have Blueprint).");
			}
			return null;
		}
	}

	public Spellbook DemandSpellbook([NotNull] BlueprintCharacterClass @class)
	{
		return null;
	}

	public Spellbook DemandSpellbook([NotNull] BlueprintSpellbook spellbook)
	{
		return null;
	}

	[CanBeNull]
	public Spellbook GetSpellbook([NotNull] BlueprintSpellbook blueprint)
	{
		return null;
	}

	public void Stop()
	{
		base.HoldState = false;
		base.Commands.InterruptAll((AbstractUnitCommand c) => !c.IsStarted);
		CombatState.LastTarget = null;
		CombatState.ManualTarget = null;
	}

	public void Hold()
	{
		base.HoldState = true;
		base.Commands.InterruptMove();
	}

	public void TryCancelCommands()
	{
		if (!base.Commands.IsRunning())
		{
			base.HoldState = false;
			base.Commands.InterruptAllInterruptible();
			CombatState.LastTarget = null;
			CombatState.ManualTarget = null;
			View.MovementAgent.Stop();
		}
	}

	public bool HasControlLossEffects()
	{
		foreach (Buff buff in base.Buffs)
		{
			foreach (EntityFactComponent component in buff.Components)
			{
				BlueprintComponent sourceBlueprintComponent = component.SourceBlueprintComponent;
				if (sourceBlueprintComponent is ChangeFaction)
				{
					return true;
				}
				if (sourceBlueprintComponent is AddCondition { Condition: UnitCondition.CantMove })
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsSummoned()
	{
		return base.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) != null;
	}

	public bool IsSummoned(out MechanicEntity caster)
	{
		caster = base.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)?.Context.MaybeCaster;
		if (caster != null)
		{
			return caster != this;
		}
		return false;
	}

	public void InitAsPet(BaseUnitEntity owner)
	{
		m_MasterRef = owner;
		OnGainPathRank(null);
		EventBus.RaiseEvent((IAbstractUnitEntity)this, (Action<IPetInitializationHandler>)delegate(IPetInitializationHandler handler)
		{
			handler.OnPetInitialized();
		}, isCheckRuntime: true);
	}

	public CompanionState? GetCompanionState()
	{
		if (base.IsDisposed)
		{
			return CompanionState.None;
		}
		return GetOptional<UnitPartCompanion>()?.State;
	}

	public bool IsUnseen()
	{
		return !Game.Instance.UnitGroups.Any((UnitGroup group) => group.IsEnemy(this) && group.Memory.ContainsVisible(this));
	}

	public bool IsCurrentUnit()
	{
		if (Game.Instance.TurnController.CurrentUnit != null)
		{
			return this == Game.Instance.TurnController.CurrentUnit;
		}
		return false;
	}

	public void PrepareRespec()
	{
	}

	public override void MarkExtra()
	{
		m_IsExtra = true;
		CombatGroup.Id = "<optimized-units>";
	}

	public void MarkSpawnFromPsychicPhenomena()
	{
		SpawnFromPsychicPhenomena = true;
	}

	public void MarkAsCloneOfMainCharacter()
	{
		m_IsCloneOfManeCharacter = true;
	}

	public void OnGainPathRank(BlueprintPath path)
	{
		Health.HitPoints.UpdateValue();
		EventBus.RaiseEvent((IBaseUnitEntity)this, (Action<IUnitGainPathRankHandler>)delegate(IUnitGainPathRankHandler h)
		{
			h.HandleUnitGainPathRank(path);
		}, isCheckRuntime: true);
	}

	protected override void OnDifficultyChanged()
	{
		base.OnDifficultyChanged();
		_ = Faction.IsPlayer;
	}

	protected override void DisposeImplementation()
	{
		bool isPreviewUnit = base.IsPreviewUnit;
		using (ContextData<DisableStatefulRandomContext>.RequestIf(isPreviewUnit))
		{
			using (ContextData<UnitHelper.DoNotCreateItems>.RequestIf(isPreviewUnit))
			{
				using (ContextData<UnitHelper.PreviewUnit>.RequestIf(isPreviewUnit))
				{
					if (isPreviewUnit && IsPet)
					{
						Game.Instance.EntityDestroyer.Destroy(Master);
					}
					base.DisposeImplementation();
				}
			}
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		if (Game.Instance.UnitMovableAreaController.TryGetInitialPosition(this, out var initialPosition))
		{
			UnitPartDeploymentPhaseInitialPosition orCreate = GetOrCreate<UnitPartDeploymentPhaseInitialPosition>();
			if (orCreate != null)
			{
				orCreate.InitialPosition = initialPosition;
			}
		}
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
		UnitPartDeploymentPhaseInitialPosition optional = GetOptional<UnitPartDeploymentPhaseInitialPosition>();
		if (optional != null)
		{
			if (IsInCombat)
			{
				Game.Instance.UnitMovableAreaController.ApplyInitialPosition(this, optional.InitialPosition);
			}
			Remove<UnitPartDeploymentPhaseInitialPosition>();
		}
	}

	public void HandleFakeSelected(bool value)
	{
		IsFakeSelected = value;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		float val2 = TimeToNextRoundTick;
		result.Append(ref val2);
		int val3 = CachedPerceptionRoll;
		result.Append(ref val3);
		bool val4 = GiveExperienceOnDeath;
		result.Append(ref val4);
		if (LastRestTime.HasValue)
		{
			TimeSpan val5 = LastRestTime.Value;
			result.Append(ref val5);
		}
		List<BlueprintUnitUpgrader> appliedUpgraders = m_AppliedUpgraders;
		if (appliedUpgraders != null)
		{
			for (int i = 0; i < appliedUpgraders.Count; i++)
			{
				Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(appliedUpgraders[i]);
				result.Append(ref val6);
			}
		}
		result.Append(MusicBossFightTypeGroup);
		result.Append(MusicBossFightTypeValue);
		EntityRef<BaseUnitEntity> obj = m_MasterRef;
		Hash128 val7 = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val7);
		return result;
	}
}
