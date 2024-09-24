using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Animation.Kingmaker;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[JsonObject(MemberSerialization.OptIn)]
public abstract class MechanicEntity : Entity, IEntityPartsManagerDelegate, IInitiativeHolder, IMechanicEntity, IEntity, IDisposable, IHashable
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private Initiative m_Initiative;

	public PartMechanicFeatures m_Features;

	private int m_GraphVersionIndex;

	private NNInfo m_CurrentNode;

	private CustomGridNode m_CurrentUnwalkableNode;

	[JsonProperty]
	public BlueprintMechanicEntityFact OriginalBlueprint { get; private set; }

	[JsonProperty]
	public BlueprintMechanicEntityFact Blueprint { get; private set; }

	[JsonProperty]
	public MechanicEntityFact MainFact { get; private set; }

	public new EntityRef<MechanicEntity> Ref => this;

	public BuffCollection Buffs { get; private set; }

	public Initiative Initiative => m_Initiative;

	public new MechanicEntityView View => (MechanicEntityView)base.View;

	public virtual float Corpulence => 0.3f;

	public virtual float Orientation => 0f;

	public virtual IntRect SizeRect => this.GetSizeRect();

	public Vector3 EyePosition => Position + LosCalculations.EyeShift;

	[CanBeNull]
	public virtual UnitMovementAgentBase MaybeMovementAgent => null;

	[CanBeNull]
	public virtual UnitAnimationManager MaybeAnimationManager => null;

	public virtual bool IsInLockControlCutscene => false;

	public PartMechanicFeatures Features
	{
		get
		{
			if (m_Features == null)
			{
				m_Features = GetRequired<PartMechanicFeatures>();
			}
			return m_Features;
		}
	}

	public bool IsInPlayerParty => this.GetCombatGroupOptional()?.IsPlayerParty ?? false;

	public bool IsPlayerFaction => this.GetFactionOptional()?.IsPlayer ?? false;

	public bool IsHelpingPlayerFaction => this.GetFactionOptional()?.IsHelpingPlayer ?? false;

	public bool IsPlayerEnemy => this.GetFactionOptional()?.IsPlayerEnemy ?? false;

	public bool IsNeutral => this.GetFactionOptional()?.Neutral ?? true;

	public bool IsSummonedMonster
	{
		get
		{
			UnitPartSummonedMonster summonedMonsterOption = this.GetSummonedMonsterOption();
			if (summonedMonsterOption == null)
			{
				return false;
			}
			return summonedMonsterOption;
		}
	}

	public virtual bool IsInCombat => this.GetCombatStateOptional()?.IsInCombat ?? false;

	public virtual bool IsDirectlyControllable => this.GetFactionOptional()?.IsDirectlyControllable ?? false;

	public bool IsConscious => this.GetLifeStateOptional()?.IsConscious ?? false;

	public bool IsDead
	{
		get
		{
			PartLifeState lifeStateOptional = this.GetLifeStateOptional();
			if (lifeStateOptional == null)
			{
				return false;
			}
			return lifeStateOptional.State == UnitLifeState.Dead;
		}
	}

	public bool IsDeadOrUnconscious
	{
		get
		{
			UnitLifeState? unitLifeState = this.GetLifeStateOptional()?.State;
			if (unitLifeState.HasValue)
			{
				UnitLifeState valueOrDefault = unitLifeState.GetValueOrDefault();
				if ((uint)(valueOrDefault - 1) <= 1u)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsAnimating => this.GetStateOptional()?.IsAnimating ?? false;

	public bool IsAble => this.GetStateOptional()?.IsAble ?? true;

	public bool CanRotate => this.GetStateOptional()?.CanRotate ?? true;

	public bool CanMove => this.GetStateOptional()?.CanMove ?? true;

	public bool CanAct => this.GetStateOptional()?.CanAct ?? true;

	public bool CanActInTurnBased => this.GetStateOptional()?.CanActInTurnBased ?? true;

	public bool CanCast => true;

	public Size Size => this.GetStateOptional()?.Size ?? OriginalSize;

	public virtual Size OriginalSize => Size.Medium;

	[CanBeNull]
	public virtual string Name
	{
		get
		{
			object obj = this.GetDescriptionOptional()?.Name;
			if (obj == null)
			{
				if (Blueprint.Name.IsNullOrEmpty())
				{
					return null;
				}
				obj = Blueprint.Name;
			}
			return (string)obj;
		}
	}

	public virtual bool IsCheater => false;

	public virtual Vector3 Forward => Quaternion.AngleAxis(Orientation, Vector3.up) * Vector3.forward;

	public virtual Type RequiredBlueprintType => typeof(BlueprintMechanicEntityFact);

	public virtual bool BlockOccupiedNodes => true;

	public NNInfo CurrentNode
	{
		get
		{
			if (m_CurrentNode.node == null || m_GraphVersionIndex != GraphParamsMechanicsCache.GraphVersionIndex)
			{
				m_CurrentNode = ((AstarPath.active != null) ? ObstacleAnalyzer.GetNearestNode(Position) : default(NNInfo));
				m_GraphVersionIndex = GraphParamsMechanicsCache.GraphVersionIndex;
			}
			return m_CurrentNode;
		}
		protected set
		{
			m_CurrentNode = value;
		}
	}

	public CustomGridNode CurrentUnwalkableNode
	{
		get
		{
			if (m_CurrentUnwalkableNode == null || m_GraphVersionIndex != GraphParamsMechanicsCache.GraphVersionIndex)
			{
				m_CurrentUnwalkableNode = ((AstarPath.active != null) ? ObstacleAnalyzer.GetNearestNodeXZUnwalkable(Position) : null);
				m_GraphVersionIndex = GraphParamsMechanicsCache.GraphVersionIndex;
			}
			return m_CurrentUnwalkableNode;
		}
		protected set
		{
			m_CurrentUnwalkableNode = value;
		}
	}

	public override Vector3 Position
	{
		get
		{
			return SizePathfindingHelper.FromViewToMechanicsPosition(this, View.Or(null)?.ViewTransform.position ?? Vector3.zero);
		}
		set
		{
			Transform transform = View.Or(null)?.ViewTransform;
			if (transform != null && transform.position != value)
			{
				transform.position = SizePathfindingHelper.FromMechanicsToViewPosition(this, value);
				OnPositionChanged();
				CurrentNode = default(NNInfo);
				CurrentUnwalkableNode = null;
			}
		}
	}

	public Vector3 Center => SizePathfindingHelper.FromMechanicsToViewPosition(this, Position);

	public virtual bool CanBeAttackedDirectly => false;

	public virtual bool IsInSquad => this.GetSquadOptional()?.IsInSquad ?? false;

	public virtual bool IsSquadLeader => this.GetSquadOptional()?.IsLeader ?? false;

	protected MechanicEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected MechanicEntity(string uniqueId, bool isInGame, [NotNull] BlueprintMechanicEntityFact blueprint)
		: base(uniqueId, isInGame)
	{
		if (blueprint == null)
		{
			throw new Exception("MechanicEntity: blueprint is missing");
		}
		CheckBlueprintType(blueprint);
		Blueprint = (OriginalBlueprint = blueprint);
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartMechanicFeatures>();
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		Buffs = Facts.EnsureFactProcessor<BuffCollection>();
		Buffs.SetSubscribedOnEventBus(base.IsInGame);
		Buffs.SetActiveForce(active: true);
		if (m_Initiative == null)
		{
			m_Initiative = new Initiative();
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		MainFact = Facts.Add(CreateMainFact(OriginalBlueprint));
		UnitPartInteractions.SetupBlueprintInteractions(this);
	}

	[CanBeNull]
	protected virtual MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return new MechanicEntityFact(blueprint);
	}

	private void CheckBlueprintType([NotNull] BlueprintMechanicEntityFact blueprint)
	{
		Type requiredBlueprintType = RequiredBlueprintType;
		if (blueprint.GetType() != requiredBlueprintType && !blueprint.GetType().IsSubclassOf(requiredBlueprintType))
		{
			throw new Exception(GetType().Name + ": invalid blueprint type " + blueprint.GetType().Name + ", expected type " + requiredBlueprintType.Name);
		}
	}

	void IEntityPartsManagerDelegate.OnPartAppears(EntityPart part)
	{
		OnPartUpdated(part, removed: false);
	}

	void IEntityPartsManagerDelegate.OnPartDisappears(EntityPart part)
	{
		OnPartUpdated(part, removed: true);
	}

	protected virtual void OnPartUpdated(EntityPart part, bool removed)
	{
	}

	public void HandleSpawn()
	{
		if (HoldingState == null)
		{
			PFLog.Default.ErrorWithReport("It is unsafe to spawn entities which not in game state yet");
		}
		try
		{
			OnSpawn();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		Buffs.SetSubscribedOnEventBus(base.IsInGame);
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		Buffs.SpawnBuffsFxs();
	}

	protected override void OnDidPostLoad()
	{
		OnDifficultyChanged();
	}

	protected override void OnPreSave()
	{
	}

	protected override void OnPostLoad()
	{
	}

	protected virtual void OnSpawn()
	{
		UpdateFogOfWarState();
	}

	protected override void OnDestroy()
	{
	}

	protected override void OnDispose()
	{
	}

	public IUnitInteraction SelectClickInteraction(BaseUnitEntity initiator)
	{
		return GetOptional<UnitPartInteractions>()?.SelectClickInteraction(initiator);
	}

	public void SetFakeBlueprint([CanBeNull] BlueprintMechanicEntityFact fake)
	{
		if (fake == null)
		{
			Blueprint = OriginalBlueprint;
			return;
		}
		CheckBlueprintType(fake);
		CheckBlueprintType(fake);
		Blueprint = fake;
	}

	public void DifficultyChanged()
	{
		OnDifficultyChanged();
	}

	protected virtual void OnDifficultyChanged()
	{
	}

	public virtual StatBaseValue GetStatBaseValue(StatType type)
	{
		return 0;
	}

	[CanBeNull]
	public T GetStatOptional<T>(StatType type) where T : ModifiableValue
	{
		PartStatsContainer optional = GetOptional<PartStatsContainer>();
		if (optional == null)
		{
			return null;
		}
		return optional.GetStatOptional<T>(type);
	}

	[CanBeNull]
	public ModifiableValue GetStatOptional(StatType type)
	{
		return GetStatOptional<ModifiableValue>(type);
	}

	[CanBeNull]
	public ModifiableValueAttributeStat GetAttributeOptional(StatType type)
	{
		return GetStatOptional<ModifiableValueAttributeStat>(type);
	}

	[CanBeNull]
	public ModifiableValueSkill GetSkillOptional(StatType type)
	{
		return GetStatOptional<ModifiableValueSkill>(type);
	}

	[CanBeNull]
	public ModifiableValueSavingThrow GetSavingThrowOptional(StatType type)
	{
		return GetStatOptional<ModifiableValueSavingThrow>(type);
	}

	public bool IsEnemy(IMechanicEntity entity)
	{
		return this.GetCombatGroupOptional()?.IsEnemy((MechanicEntity)entity) ?? false;
	}

	public bool IsAlly(IMechanicEntity entity)
	{
		return this.GetCombatGroupOptional()?.IsAlly(entity) ?? false;
	}

	public bool CanAttack(MechanicEntity entity)
	{
		return this.GetCombatGroupOptional()?.CanAttack(entity) ?? true;
	}

	[CanBeNull]
	public virtual ItemEntityWeapon GetFirstWeapon()
	{
		return null;
	}

	[CanBeNull]
	public virtual ItemEntityWeapon GetPrimaryHandWeapon()
	{
		return null;
	}

	[CanBeNull]
	public virtual ItemEntityWeapon GetSecondaryHandWeapon()
	{
		return null;
	}

	public bool HasCondition(UnitCondition condition)
	{
		return this.GetStateOptional()?.HasCondition(condition) ?? false;
	}

	public bool HasConditionImmunity(UnitCondition condition)
	{
		return this.GetStateOptional()?.HasConditionImmunity(condition) ?? false;
	}

	public bool HasLOS(MechanicEntity entity)
	{
		return this.GetVisionOptional()?.HasLOS(entity) ?? false;
	}

	[CanBeNull]
	public EntityFact AddFact([CanBeNull] BlueprintMechanicEntityFact blueprint, MechanicsContext parentContext = null)
	{
		if (blueprint == null)
		{
			return null;
		}
		return Facts.Add(blueprint.CreateFact(parentContext, this, null));
	}

	public override string ToString()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append(GetType().Name);
		builder.Append('[');
		builder.Append(Blueprint.name);
		builder.Append(']');
		if (View != null)
		{
			builder.Append('[');
			builder.Append(View.name);
			builder.Append(']');
		}
		builder.Append('#');
		builder.Append(base.UniqueId);
		return builder.ToString();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<Initiative>.GetHash128(m_Initiative);
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(OriginalBlueprint);
		result.Append(ref val3);
		Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<MechanicEntityFact>.GetHash128(MainFact);
		result.Append(ref val5);
		return result;
	}
}
public abstract class MechanicEntity<TBlueprint> : MechanicEntity, IHashable where TBlueprint : BlueprintMechanicEntityFact
{
	public new TBlueprint OriginalBlueprint => (TBlueprint)base.OriginalBlueprint;

	public new TBlueprint Blueprint => (TBlueprint)base.Blueprint;

	public override Type RequiredBlueprintType => typeof(TBlueprint);

	protected MechanicEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected MechanicEntity(string uniqueId, bool isInGame, TBlueprint blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
