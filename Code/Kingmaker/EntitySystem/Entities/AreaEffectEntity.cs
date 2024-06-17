using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public class AreaEffectEntity : MechanicEntity<BlueprintAbilityAreaEffect>, IAreaHandler, ISubscriber, IAreaEffectEntity, IMechanicEntity, IEntity, IDisposable, IEntityPositionChangedHandler, ISubscriber<IEntity>, IHashable
{
	public interface IUnitWithinBoundsHandler
	{
		HashSet<UnitReference> Entered { get; }

		HashSet<UnitReference> Exited { get; }

		void ClearDelta();
	}

	public struct UnitInfo : IEquatable<UnitInfo>, IHashable
	{
		[JsonProperty]
		public UnitReference Reference;

		[JsonProperty(IsReference = false)]
		public Vector3 PreviousNodePosition;

		public bool IsValid => Reference.Entity.ToBaseUnitEntity()?.IsInState ?? false;

		public UnitInfo(UnitReference reference)
		{
			this = default(UnitInfo);
			Reference = reference;
		}

		public bool Equals(UnitInfo other)
		{
			return Reference.Equals(other.Reference);
		}

		public override bool Equals(object obj)
		{
			if (obj is UnitInfo other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Reference.GetHashCode();
		}

		public Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			UnitReference obj = Reference;
			Hash128 val = UnitReferenceHasher.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref PreviousNodePosition);
			return result;
		}
	}

	private class SpellResistanceResult
	{
		[JsonProperty]
		public UnitReference UnitRef;

		[JsonProperty]
		public int Value;

		[JsonProperty]
		public bool Immunity;

		[JsonProperty]
		public bool Resisted;
	}

	[JsonProperty]
	private readonly MechanicsContext m_Context;

	[JsonProperty]
	private readonly TargetWrapper m_Target;

	[JsonProperty]
	private readonly TimeSpan m_CreationTime;

	[JsonProperty]
	private bool m_OnUnit;

	[JsonProperty]
	public bool ForceEnded;

	[JsonProperty]
	private bool m_CanAffectAllies = true;

	[JsonProperty]
	private List<UnitInfo> m_UnitsInside = new List<UnitInfo>();

	private List<UnitReference> m_UnitsNotInside = new List<UnitReference>();

	[JsonProperty]
	private TimeSpan? m_CasterDisappearTime;

	[JsonProperty]
	[CanBeNull]
	public readonly Rounds? Duration;

	private bool m_ForceUpdate;

	private readonly Predicate<UnitReference> m_IsMovedFromOutsideToTheVoidDelegate;

	private readonly Predicate<UnitInfo> m_IsMovedFromInsideToTheVoidDelegate;

	private readonly Predicate<UnitInfo> m_IsMovedFromInsideToOutsideDelegate;

	private readonly Predicate<UnitReference> m_IsMovedFromOutsideToInsideDelegate;

	[JsonProperty]
	public Rounds Lifetime { get; private set; }

	[JsonProperty]
	public EntityFactRef SourceFact { get; set; }

	public new AreaEffectView View => (AreaEffectView)base.View;

	public NodeList CoveredNodes => View.Shape.CoveredNodes;

	public IEnumerable<BaseUnitEntity> InGameUnitsInside => from ur in m_UnitsInside.Select((UnitInfo ui) => ui.Reference).Where(delegate(UnitReference ur)
		{
			BaseUnitEntity baseUnitEntity = ur.Entity.ToBaseUnitEntity();
			return baseUnitEntity != null && baseUnitEntity.IsInGame && baseUnitEntity.IsInState;
		})
		select ur.ToBaseUnitEntity();

	public bool IsEnded
	{
		get
		{
			if (!ForceEnded)
			{
				Rounds lifetime = Lifetime;
				Rounds? duration = Duration;
				return lifetime >= duration;
			}
			return true;
		}
	}

	public MechanicsContext Context => m_Context;

	public bool AffectEnemies => base.Blueprint.AffectEnemies;

	public bool AggroEnemies => base.Blueprint.AggroEnemies;

	public override Vector3 Position => View.Or(null)?.ViewTransform.position ?? m_Target.Point;

	public override bool IsSuppressible => true;

	private bool IsInTurnBasedMode => Game.Instance.TurnController.TurnBasedModeActive;

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => base.Blueprint.FXSettings;

	public IUnitWithinBoundsHandler UnitHandler
	{
		get
		{
			if (!base.Blueprint.IsAllArea)
			{
				return GetRequired<AreaEffectBoundsPart>();
			}
			return GetRequired<AreaEffectUnboundPart>();
		}
	}

	public NodeList GetPatternCoveredNodes()
	{
		if (!base.Blueprint.SavePersistentArea)
		{
			return NodeList.Empty;
		}
		return View.Shape.CoveredNodes;
	}

	public AreaEffectEntity([NotNull] AreaEffectView view, [CanBeNull] MechanicsContext context, [NotNull] BlueprintAbilityAreaEffect blueprint, [NotNull] TargetWrapper target, TimeSpan creationTime, TimeSpan? duration, bool onUnit)
		: base(view.UniqueId, view.IsInGameBySettings, blueprint)
	{
		if (onUnit && target.Entity == null)
		{
			throw new Exception("Area effect is attached to unit, but unit is missing");
		}
		m_Context = context ?? new MechanicsContext(this, null, blueprint, null, m_Target);
		m_Target = target;
		m_CreationTime = creationTime;
		Duration = duration?.ToRounds();
		m_OnUnit = onUnit;
		m_Context.Recalculate();
		base.Blueprint.HandleSpawn(m_Context, this);
		m_IsMovedFromOutsideToTheVoidDelegate = IsMovedFromOutsideToTheVoid;
		m_IsMovedFromInsideToTheVoidDelegate = IsMovedFromInsideToTheVoid;
		m_IsMovedFromInsideToOutsideDelegate = IsMovedFromInsideToOutside;
		m_IsMovedFromOutsideToInsideDelegate = IsMovedFromOutsideToInside;
		m_ForceUpdate = true;
	}

	[UsedImplicitly]
	protected AreaEffectEntity(JsonConstructorMark _)
		: base(_)
	{
		m_IsMovedFromOutsideToTheVoidDelegate = IsMovedFromOutsideToTheVoid;
		m_IsMovedFromInsideToTheVoidDelegate = IsMovedFromInsideToTheVoid;
		m_IsMovedFromInsideToOutsideDelegate = IsMovedFromInsideToOutside;
		m_IsMovedFromOutsideToInsideDelegate = IsMovedFromOutsideToInside;
		m_ForceUpdate = true;
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		if (!base.Blueprint.IsAllArea)
		{
			GetOrCreate<AreaEffectBoundsPart>();
			Remove<AreaEffectUnboundPart>();
		}
		else
		{
			GetOrCreate<AreaEffectUnboundPart>();
			Remove<AreaEffectBoundsPart>();
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		m_Context.Recalculate();
		base.Blueprint.HandleSpawn(m_Context, this);
		UpdateCombatInitiative();
	}

	protected override IEntityViewBase CreateViewForData()
	{
		if (m_Target == null || (!m_OnUnit && m_Target.NearestNode == null))
		{
			PFLog.Default.Error($"Target for AreaEffectEntity {base.Blueprint} is null or empty! Skipping View create");
			return null;
		}
		AreaEffectView areaEffectView = new GameObject($"AreaEffect [{base.Blueprint}]").AddComponent<AreaEffectView>();
		areaEffectView.OnUnit = m_OnUnit;
		areaEffectView.InitAtRuntime(m_Context, base.Blueprint, m_Target, m_CreationTime, Duration?.Seconds);
		areaEffectView.ViewTransform.position = m_Target.Point;
		areaEffectView.ViewTransform.rotation = Quaternion.Euler(0f, m_Target.Orientation, 0f);
		return areaEffectView;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		View.UpdatePatternIfNecessary(base.Blueprint);
		UpdateFxs();
	}

	protected override void OnViewWillDetach()
	{
		base.OnViewWillDetach();
		View.RemoveFxs();
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (!SourceFact.IsEmpty && SourceFact.Fact == null)
		{
			PFLog.Default.ErrorWithReport($"SourceFact of area effect is missing: {base.Blueprint}");
			ForceEnd();
		}
	}

	protected override void OnDispose()
	{
		EventBus.RaiseEvent((IAreaEffectEntity)this, (Action<IAreaEffectHandler>)delegate(IAreaEffectHandler h)
		{
			h.HandleAreaEffectDestroyed();
		}, isCheckRuntime: true);
		base.OnDispose();
	}

	public void UpdateCombatInitiative()
	{
		MechanicEntity mechanicEntity = (TurnController.IsInTurnBasedCombat() ? Context.MaybeCaster : null);
		Initiative initiative = ((mechanicEntity != null && mechanicEntity.IsInCombat) ? mechanicEntity : null)?.Initiative;
		base.Initiative.Value = initiative?.Value ?? 0f;
		base.Initiative.Order = initiative?.Order ?? 0;
		if (initiative != null)
		{
			MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
			if (currentUnit != null && initiative.TurnOrderPriority >= currentUnit.Initiative.TurnOrderPriority)
			{
				base.Initiative.LastTurn = Game.Instance.TurnController.GameRound;
			}
		}
	}

	public void Tick()
	{
		using (ProfileScope.New("EndIfNecessary"))
		{
			EndEffectIfNecessary();
		}
		if (!IsEnded)
		{
			UpdateViewAndUnits();
			using (ProfileScope.New("HandleTick"))
			{
				base.Blueprint.HandleTick(m_Context, this);
			}
		}
		if (IsEnded)
		{
			HandleEnd();
		}
	}

	public void UpdatePosition()
	{
		using (ProfileScope.NewScope("UpdatePosition"))
		{
			if (m_OnUnit && View != null)
			{
				View.ViewTransform.position = m_Target.Point;
				View.ViewTransform.rotation = Quaternion.Euler(0f, m_Target.Orientation, 0f);
				View.UpdatePatternIfNecessary(base.Blueprint);
			}
			GetRequired<AreaEffectBoundsPart>().Tick();
		}
	}

	public void UpdateViewAndUnits()
	{
		Bounds bounds = View.Shape.GetBounds();
		if (!base.Blueprint.IsAllArea)
		{
			UpdatePosition();
		}
		Bounds bounds2 = View.Shape.GetBounds();
		bool isInCombat = Game.Instance.Player.IsInCombat;
		bool flag = bounds2 != bounds;
		using (ProfileScope.New("UpdateUnits"))
		{
			if (!isInCombat || flag || m_ForceUpdate)
			{
				m_ForceUpdate = false;
				UpdateUnits();
			}
		}
	}

	public void NextRound()
	{
		using (ProfileScope.New("NextRound"))
		{
			Lifetime += 1.Rounds();
			base.Blueprint.HandleRound(m_Context, this);
		}
		if (IsEnded)
		{
			HandleEnd();
		}
		m_ForceUpdate = true;
	}

	private void HandleEnd()
	{
		base.Blueprint.HandleEnd(m_Context, this);
		foreach (UnitInfo item in m_UnitsInside)
		{
			BlueprintAbilityAreaEffect blueprint = base.Blueprint;
			MechanicsContext context = m_Context;
			UnitReference reference = item.Reference;
			blueprint.HandleUnitExit(context, this, reference.Entity.ToIBaseUnitEntity());
		}
		m_UnitsInside.Clear();
		m_UnitsNotInside.Clear();
		Game.Instance.EntityDestroyer.Destroy(this);
	}

	private void EndEffectIfNecessary()
	{
		if (Context?.MaybeCaster == null || (m_OnUnit && m_Target.Entity == null))
		{
			ForceEnd();
			return;
		}
		MechanicsContext context = Context;
		bool? obj;
		if (context == null)
		{
			obj = null;
		}
		else
		{
			MechanicEntity maybeCaster = context.MaybeCaster;
			obj = ((maybeCaster != null) ? new bool?(!maybeCaster.IsInState) : null);
		}
		bool? flag = obj;
		if (flag.GetValueOrDefault() && m_OnUnit)
		{
			MechanicEntity entity = m_Target.Entity;
			if (entity != null && !entity.IsInState && base.IsInState)
			{
				ForceEnd();
				return;
			}
		}
		if (m_Context.SourceAbility != null && (m_Context.MaybeCaster == null || m_Context.MaybeCaster.IsDead) && !Game.Instance.Player.IsInCombat)
		{
			TimeSpan timeSpan = BlueprintRoot.Instance.AreaEffectAutoDestroySeconds.Seconds();
			TimeSpan gameTime = Game.Instance.TimeController.GameTime;
			if (!m_CasterDisappearTime.HasValue)
			{
				m_CasterDisappearTime = gameTime;
			}
			else if (gameTime - m_CasterDisappearTime.Value >= timeSpan)
			{
				ForceEnd();
			}
		}
	}

	private bool IsMovedFromOutsideToTheVoid(UnitReference unit)
	{
		return UnitHandler.Exited.Contains(unit);
	}

	private bool IsMovedFromInsideToTheVoid(UnitInfo unit)
	{
		if (!UnitHandler.Exited.Contains(unit.Reference))
		{
			return false;
		}
		using (ProfileScope.New("HandleUnitExit"))
		{
			base.Blueprint.HandleUnitExit(m_Context, this, unit.Reference.Entity.ToIBaseUnitEntity());
		}
		return true;
	}

	private bool IsMovedFromInsideToOutside(UnitInfo unit)
	{
		IAbstractUnitEntity entity = unit.Reference.Entity;
		if (entity == null || entity.IsDisposed)
		{
			return true;
		}
		if (m_UnitsNotInside.HasItem((UnitReference i) => i == unit.Reference))
		{
			return true;
		}
		if (!IsEnded && ShouldUnitBeInside(entity.ToIBaseUnitEntity()))
		{
			return false;
		}
		m_UnitsNotInside.Add(unit.Reference);
		using (ProfileScope.New("HandleUnitExit"))
		{
			base.Blueprint.HandleUnitExit(m_Context, this, entity.ToIBaseUnitEntity());
		}
		return true;
	}

	private bool IsMovedFromOutsideToInside(UnitReference unit)
	{
		if (IsEnded)
		{
			return false;
		}
		IAbstractUnitEntity entity = unit.Entity;
		if (entity == null || entity.IsDisposed)
		{
			return false;
		}
		if (m_UnitsInside.HasItem((UnitInfo i) => i.Reference == unit))
		{
			return true;
		}
		if (!ShouldUnitBeInside(unit.Entity.ToIBaseUnitEntity()))
		{
			return false;
		}
		m_UnitsInside.Add(new UnitInfo(unit)
		{
			PreviousNodePosition = unit.Entity.ToBaseUnitEntity().CurrentUnwalkableNode.Vector3Position
		});
		using (ProfileScope.New("HandleUnitEnter"))
		{
			base.Blueprint.HandleUnitEnter(m_Context, this, unit.Entity.ToBaseUnitEntity());
		}
		return true;
	}

	private void UpdateUnits()
	{
		m_UnitsNotInside.RemoveAll(m_IsMovedFromOutsideToTheVoidDelegate);
		m_UnitsInside.RemoveAll(m_IsMovedFromInsideToTheVoidDelegate);
		int count = m_UnitsNotInside.Count;
		foreach (UnitReference item in UnitHandler.Entered)
		{
			m_UnitsNotInside.Add(item);
		}
		m_UnitsNotInside.Sort(count, m_UnitsNotInside.Count - count, UnitReferenceComparer.Instance);
		m_UnitsInside.RemoveAll(m_IsMovedFromInsideToOutsideDelegate);
		m_UnitsNotInside.RemoveAll(m_IsMovedFromOutsideToInsideDelegate);
		UnitHandler.ClearDelta();
		for (int i = 0; i < m_UnitsInside.Count; i++)
		{
			UnitInfo unitInfo = m_UnitsInside[i];
			BaseUnitEntity baseUnitEntity = unitInfo.Reference.Entity.ToBaseUnitEntity();
			if (!baseUnitEntity.Movable.HasMotionThisSimulationTick)
			{
				continue;
			}
			Vector3 vector3Position = baseUnitEntity.CurrentUnwalkableNode.Vector3Position;
			if ((vector3Position - unitInfo.PreviousNodePosition).sqrMagnitude > 1f && (vector3Position - baseUnitEntity.Position).sqrMagnitude < 0.1f)
			{
				using (ProfileScope.New("HandleUnitMove"))
				{
					base.Blueprint.HandleUnitMove(m_Context, this, baseUnitEntity);
				}
				m_UnitsInside[i] = new UnitInfo(baseUnitEntity.FromBaseUnitEntity())
				{
					PreviousNodePosition = vector3Position
				};
			}
		}
	}

	private bool ShouldUnitBeInside(IBaseUnitEntity unitEntity)
	{
		return ShouldUnitBeInside((BaseUnitEntity)unitEntity);
	}

	private bool ShouldUnitBeInside(BaseUnitEntity unit)
	{
		using (ProfileScope.NewScope("ShouldUnitBeInside"))
		{
			return unit.IsInGame && (!unit.IsInFogOfWar || unit.IsInCombat || unit.AwakeTimer > 0f) && (!base.Blueprint.IgnoreSleepingUnits || !unit.IsSleeping) && (unit.LifeState.IsConscious || base.Blueprint.AffectDead) && IsSuitableTargetType(unit) && (base.Blueprint.IsAllArea || (Contains(unit) && !LineOfSightGeometry.Instance.HasObstacle(View.ViewTransform.position, unit.Position))) && !AbstractUnitCommand.CommandTargetUntargetable(this, unit) && (!unit.Features.Flying || !Context.SpellDescriptor.HasAnyFlag(SpellDescriptor.Ground));
		}
	}

	public bool IsSuitableTargetType(BaseUnitEntity unit)
	{
		MechanicEntity mechanicEntity = Context?.MaybeCaster;
		if (mechanicEntity == null)
		{
			return false;
		}
		bool flag = mechanicEntity.IsEnemy(unit) || unit.Faction.Neutral;
		if (!flag || !base.Blueprint.CanTargetEnemies)
		{
			if (!flag && base.Blueprint.CanTargetAllies)
			{
				return m_CanAffectAllies;
			}
			return false;
		}
		return true;
	}

	public bool Overlaps(IEnumerable<CustomGridNodeBase> nodes)
	{
		foreach (CustomGridNodeBase node in nodes)
		{
			if (node == null)
			{
				throw new Exception("AreaEffectEntity.Overlaps: Cannot check an empty node for overlap");
			}
			if (Contains(node))
			{
				return true;
			}
		}
		return false;
	}

	public DestructibleEntity[] GetAllDestructibleEntityInside()
	{
		List<DestructibleEntity> list = TempList.Get<DestructibleEntity>();
		foreach (DestructibleEntity destructibleEntity in Game.Instance.State.DestructibleEntities)
		{
			if (Contains(destructibleEntity.Position))
			{
				list.Add(destructibleEntity);
			}
		}
		return list.ToArray();
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		foreach (UnitInfo item in m_UnitsInside)
		{
			IAbstractUnitEntity entity = item.Reference.Entity;
			if (entity != null)
			{
				base.Blueprint.HandleUnitExit(m_Context, this, entity.ToIBaseUnitEntity());
			}
		}
		m_UnitsInside.Clear();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
	}

	public void ForceEnd()
	{
		ForceEnded = true;
		EventBus.RaiseEvent((IAreaEffectEntity)this, (Action<IAreaEffectForceEndHandler>)delegate(IAreaEffectForceEndHandler h)
		{
			h.HandleAreaEffectForceEndRequested();
		}, isCheckRuntime: true);
	}

	public bool Contains(BaseUnitEntity unit)
	{
		return View.Contains(unit);
	}

	public bool Contains(Vector3 point)
	{
		return View.Contains(point);
	}

	public bool Contains(CustomGridNodeBase node)
	{
		return View.Contains(node);
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		UpdateFxs();
		m_ForceUpdate = true;
	}

	private void UpdateFxs()
	{
		if (!(View == null))
		{
			if (base.IsInGame)
			{
				View.SpawnFxs();
			}
			else
			{
				View.RemoveFxs();
			}
		}
	}

	public void HandleEntityPositionChanged()
	{
		m_ForceUpdate = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicsContext>.GetHash128(m_Context);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<TargetWrapper>.GetHash128(m_Target);
		result.Append(ref val3);
		TimeSpan val4 = m_CreationTime;
		result.Append(ref val4);
		result.Append(ref m_OnUnit);
		result.Append(ref ForceEnded);
		result.Append(ref m_CanAffectAllies);
		List<UnitInfo> unitsInside = m_UnitsInside;
		if (unitsInside != null)
		{
			for (int i = 0; i < unitsInside.Count; i++)
			{
				UnitInfo obj = unitsInside[i];
				Hash128 val5 = StructHasher<UnitInfo>.GetHash128(ref obj);
				result.Append(ref val5);
			}
		}
		if (m_CasterDisappearTime.HasValue)
		{
			TimeSpan val6 = m_CasterDisappearTime.Value;
			result.Append(ref val6);
		}
		if (Duration.HasValue)
		{
			Rounds val7 = Duration.Value;
			result.Append(ref val7);
		}
		Rounds val8 = Lifetime;
		result.Append(ref val8);
		EntityFactRef obj2 = SourceFact;
		Hash128 val9 = StructHasher<EntityFactRef>.GetHash128(ref obj2);
		result.Append(ref val9);
		return result;
	}
}
