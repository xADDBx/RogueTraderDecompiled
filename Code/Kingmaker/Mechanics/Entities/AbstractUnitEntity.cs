using System;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Kingmaker.Visual.HitSystem;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Mechanics.Entities;

public abstract class AbstractUnitEntity : MechanicEntity<BlueprintUnit>, PartStatsContainer.IOwner, IEntityPartOwner<PartStatsContainer>, IEntityPartOwner, PartUnitCommands.IOwner, IEntityPartOwner<PartUnitCommands>, PartUnitAsks.IOwner, IEntityPartOwner<PartUnitAsks>, PartMovable.IOwner, IEntityPartOwner<PartMovable>, PartUnitViewSettings.IOwner, IEntityPartOwner<PartUnitViewSettings>, PartHealth.IOwner, IEntityPartOwner<PartHealth>, PartLifeState.IOwner, IEntityPartOwner<PartLifeState>, PartUnitState.IOwner, IEntityPartOwner<PartUnitState>, IAbstractUnitEntity, IMechanicEntity, IEntity, IDisposable, IHashable
{
	public interface IUnitAsleepHandler : ISubscriber<IEntity>, ISubscriber
	{
		void OnIsSleepingChanged(bool sleeping);
	}

	public PartStatsContainer m_Stats;

	public PartUnitCommands m_Commands;

	public PartMovable m_Movable;

	public PartLifeState m_LifeState;

	[JsonProperty]
	private Vector3 m_Position;

	[JsonProperty]
	private float m_Orientation;

	[JsonProperty]
	public readonly CountableFlag Sleepless = new CountableFlag();

	private float m_DesiredOrientation;

	public readonly ObservableCountFlag Passive = new ObservableCountFlag();

	public readonly CountableFlag ControlledByDirector = new CountableFlag();

	public const float PositionChangedThreshold = 1E-05f;

	private bool m_IsSleeping;

	public PartStatsContainer Stats
	{
		get
		{
			if (m_Stats == null)
			{
				m_Stats = GetRequired<PartStatsContainer>();
			}
			return m_Stats;
		}
	}

	public PartUnitCommands Commands
	{
		get
		{
			if (m_Commands == null)
			{
				m_Commands = GetRequired<PartUnitCommands>();
			}
			return m_Commands;
		}
	}

	public PartMovable Movable
	{
		get
		{
			if (m_Movable == null)
			{
				m_Movable = GetRequired<PartMovable>();
			}
			return m_Movable;
		}
	}

	public abstract PartUnitAsks Asks { get; }

	public abstract PartUnitViewSettings ViewSettings { get; }

	public abstract PartHealth Health { get; }

	public PartLifeState LifeState
	{
		get
		{
			if (m_LifeState == null)
			{
				m_LifeState = GetRequired<PartLifeState>();
			}
			return m_LifeState;
		}
	}

	public abstract PartUnitState State { get; }

	[CanBeNull]
	public abstract PartSavedRagdollState SavedRagdoll { get; }

	[CanBeNull]
	public abstract SavedDismembermentState SavedDismemberment { get; }

	[JsonProperty(IsReference = false)]
	public Vector3 SpawnPosition { get; set; }

	[JsonProperty]
	public bool HoldState { get; set; }

	[JsonProperty]
	public float DesiredOrientation
	{
		get
		{
			return m_DesiredOrientation;
		}
		set
		{
			if (m_DesiredOrientation != value)
			{
				m_DesiredOrientation = value;
			}
		}
	}

	[JsonProperty(IsReference = false)]
	public StatefulRandom Random { get; private set; }

	[JsonProperty]
	public float FlyHeight { get; set; }

	public virtual bool IsExtra => false;

	[CanBeNull]
	public CutsceneControlledUnit CutsceneControlledUnit { get; set; }

	public float AwakeTimer { get; set; } = -1f;


	public new AbstractUnitEntityView View => (AbstractUnitEntityView)base.View;

	public override bool IsAffectedByFogOfWar => false;

	public virtual bool IsDeadAndHasLoot => false;

	public virtual bool LootViewed => false;

	public bool IsPreviewUnit => IsPreview;

	public virtual bool AreHandsBusyWithAnimation => false;

	public UnitAnimationManager AnimationManager => View.AnimationManager;

	public override UnitAnimationManager MaybeAnimationManager => ObjectExtensions.Or(View, null)?.AnimationManager;

	public UnitMovementAgentBase MovementAgent => ObjectExtensions.Or(MaybeMovementAgent, null) ?? throw new Exception("MovementAgent is missing");

	public override UnitMovementAgentBase MaybeMovementAgent => ObjectExtensions.Or(View, null)?.MovementAgent;

	public GraphNode PreviousNode { get; private set; }

	public override float Orientation => m_Orientation;

	public override Vector3 Position
	{
		get
		{
			return m_Position;
		}
		set
		{
			if ((m_Position - value).sqrMagnitude > 1E-05f)
			{
				m_Position = value;
				Wake();
				OnPositionChanged();
				PreviousNode = base.CurrentNode.node;
				base.CurrentNode = default(NNInfo);
				if (PreviousNode == null || PreviousNode != base.CurrentNode.node)
				{
					OnNodeChanged();
				}
				base.CurrentUnwalkableNode = null;
			}
		}
	}

	public Vector3 OrientationDirection => Quaternion.Euler(0f, Orientation, 0f) * Vector3.forward;

	public string CharacterName => this.GetDescriptionOptional()?.Name ?? base.Blueprint.Name;

	public Transform ViewTransform => View.ViewTransform;

	public Gender Gender => this.GetDescriptionOptional()?.Gender ?? base.Blueprint.Gender;

	public int HitPointsLeft => this.GetHealthOptional()?.HitPointsLeft ?? 1;

	public bool IsSleeping
	{
		get
		{
			return m_IsSleeping;
		}
		set
		{
			if (m_IsSleeping != value)
			{
				m_IsSleeping = value;
				if ((bool)View)
				{
					View.UpdateViewActive();
				}
				NotifyIsSleepingChanged(value);
			}
		}
	}

	public bool FreezeOutsideCamera { get; set; }

	public ShieldType ShieldType { get; set; }

	public SurfaceType SurfaceType => base.Blueprint.VisualSettings.SurfaceType;

	public override bool IsViewActive
	{
		get
		{
			if (!base.IsViewActive)
			{
				return false;
			}
			if (IsSleeping && !LifeState.IsDeathRevealed)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsAwake => Game.Instance.State.AllAwakeUnits.Contains(this);

	public virtual float GetWarhammerMovementApPerCellThreateningArea()
	{
		return base.Blueprint.WarhammerMovementApPerCellThreateningArea;
	}

	protected virtual void OnNodeChanged()
	{
	}

	private void NotifyIsSleepingChanged(bool value)
	{
		EventBus.RaiseEvent((IEntity)this, (Action<IUnitAsleepHandler>)delegate(IUnitAsleepHandler v)
		{
			v.OnIsSleepingChanged(value);
		}, isCheckRuntime: true);
	}

	protected AbstractUnitEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected AbstractUnitEntity(string uniqueId, bool isInGame, BlueprintUnit blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		if (ObjectExtensions.Or(View, null)?.MovementAgent != null)
		{
			View.MovementAgent.Stop();
		}
		using (ContextData<SpawnedUnitData>.Request().Setup(this))
		{
			EventBus.RaiseEvent((IAbstractUnitEntity)this, (Action<IUnitSpawnHandler>)delegate(IUnitSpawnHandler h)
			{
				h.HandleUnitSpawned();
			}, isCheckRuntime: true);
		}
	}

	protected override void OnPreSave()
	{
		base.OnPreSave();
		if (View != null && View.RigidbodyController != null)
		{
			GetOrCreate<PartSavedRagdollState>().SaveRagdollState(View.RigidbodyController);
		}
		if (View != null && View.DismembermentManager != null && View.DismembermentManager.Dismembered)
		{
			GetOrCreate<SavedDismembermentState>().SaveDismembermentState(View.DismembermentManager);
		}
	}

	protected override void OnDestroy()
	{
		EventBus.RaiseEvent((IAbstractUnitEntity)this, (Action<IUnitHandler>)delegate(IUnitHandler h)
		{
			h.HandleUnitDestroyed();
		}, isCheckRuntime: true);
		base.OnDestroy();
	}

	protected override void OnDispose()
	{
		if (Health != null)
		{
			Health.LastHandledDamage = null;
		}
		if (!ContextData<SceneEntitiesState.DisposeInProgress>.Current && base.Blueprint?.AssetGuid == "00ac5fe6a92a434aa89518306180b30e")
		{
			PFLog.History.System.Log("HighFactotum disposed\n" + StackTraceUtility.ExtractStackTrace());
			PFLog.System.ErrorWithReport("HighFactotum disposed. It's OK if it is Rogue Trader's hallucinations, otherwise report to programmers IMMEDIATELY");
		}
		if (Application.isPlaying && LoadingProcess.Instance.CurrentProcessTag == LoadingProcessTag.ReloadMechanics)
		{
			CutsceneControlledUnit.GetControllingPlayer(this)?.Stop();
		}
		base.OnDispose();
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		uint seed = (((bool)ContextData<UnitHelper.PreviewUnit>.Current || IsPreviewUnit) ? PFStatefulRandom.PreviewUnitRandom.uintValue : PFStatefulRandom.UnitRandom.uintValue);
		if (Random == null)
		{
			StatefulRandom statefulRandom2 = (Random = new StatefulRandom("Unit " + base.UniqueId, seed));
		}
	}

	public void Wake(float time = 0f)
	{
		AwakeTimer = Mathf.Max(time, AwakeTimer);
	}

	public void Translocate(Vector3 position, float? orientation)
	{
		Position = position;
		if (orientation.HasValue)
		{
			SetOrientation(orientation.Value);
		}
		if (View != null)
		{
			View.ViewTransform.position = position;
			View.ForcePlaceAboveGround();
			if (orientation.HasValue)
			{
				View.ViewTransform.rotation = Quaternion.Euler(0f, orientation.Value, 0f);
			}
		}
	}

	public void SetOrientation(float value)
	{
		m_Orientation = value;
		DesiredOrientation = value;
	}

	public void UpdateSlowRotation(float maxAngle)
	{
		_ = m_Orientation;
		m_Orientation = Mathf.MoveTowardsAngle(m_Orientation, DesiredOrientation, maxAngle);
	}

	public float GetLookAtAngle(Vector3 point)
	{
		if (point == Position)
		{
			return DesiredOrientation;
		}
		Vector3 forward = point - Position;
		forward.y = 0f;
		return Quaternion.LookRotation(forward).eulerAngles.y;
	}

	public void LookAt(Vector3 point)
	{
		DesiredOrientation = GetLookAtAngle(point);
	}

	public void ForceLookAt(Vector3 point)
	{
		SetOrientation(GetLookAtAngle(point));
		if (View != null && (Game.Instance.IsPaused || !View.IsVisible))
		{
			View.ViewTransform.rotation = Quaternion.Euler(0f, Orientation, 0f);
		}
	}

	public void SetPositionWithoutWaking(Vector3 pos)
	{
		float awakeTimer = AwakeTimer;
		Position = pos;
		AwakeTimer = awakeTimer;
	}

	public abstract void MarkExtra();

	public void UpdateSizeModifiers()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Position);
		result.Append(ref m_Orientation);
		Vector3 val2 = SpawnPosition;
		result.Append(ref val2);
		bool val3 = HoldState;
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<CountableFlag>.GetHash128(Sleepless);
		result.Append(ref val4);
		float val5 = DesiredOrientation;
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<StatefulRandom>.GetHash128(Random);
		result.Append(ref val6);
		float val7 = FlyHeight;
		result.Append(ref val7);
		return result;
	}
}
