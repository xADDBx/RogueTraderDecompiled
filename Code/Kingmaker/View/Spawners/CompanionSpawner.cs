using System;
using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("dfea570039374dd99e2e6cf487f9add8")]
public class CompanionSpawner : UnitSpawnerBase, IPartyHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IAddInspectorGUI, ICompanionChangeHandler, IEtudesUpdateHandler
{
	private enum LogContext
	{
		Unknown,
		SceneInit,
		SpawnUnit,
		AddedToParty,
		Activated,
		RemovedFromParty,
		CapitalModeChanged,
		Recruited,
		Unrecruited,
		EtudeUpdate
	}

	private new class MyData : UnitSpawnerBase.MyData
	{
		private bool m_NeedInitOnPlace;

		[JsonProperty]
		public bool DidSpawnOnce { get; private set; }

		public MyData(EntityViewBase view)
			: base(view)
		{
		}

		private MyData(JsonConstructorMark _)
			: base(_)
		{
		}

		public override bool ShouldProcessActivation(bool alsoRaiseDead)
		{
			return true;
		}

		public void OnPlaceCompanion(bool didPlace)
		{
			AbstractUnitEntity unit = base.SpawnedUnit.Entity;
			if (unit == null || unit.WillBeDestroyed || unit.Destroyed)
			{
				return;
			}
			if (!didPlace)
			{
				UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
				if (optional != null && optional.IsControllableInParty() && !m_NeedInitOnPlace)
				{
					Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
					{
						d.OnDispose(unit);
					});
					m_NeedInitOnPlace = true;
				}
			}
			if (didPlace && m_NeedInitOnPlace)
			{
				Parts.GetAll<IUnitInitializer>().ForEach(delegate(IUnitInitializer d)
				{
					d.OnInitialize(unit);
				});
				m_NeedInitOnPlace = false;
			}
		}

		protected override void OnSpawned()
		{
			base.OnSpawned();
			m_NeedInitOnPlace = false;
			DidSpawnOnce = true;
		}
	}

	[SerializeField]
	private bool m_SpawnWhenNone;

	[SerializeField]
	private bool m_SpawnWhenRemote;

	[SerializeField]
	private bool m_SpawnWhenInCapital;

	[SerializeField]
	private bool m_SpawnWhenEx = true;

	[SerializeField]
	private bool m_HideIfDead = true;

	[SerializeField]
	private ConditionsReference ControlCondition;

	[SerializeField]
	private ConditionsReference ShowCondition;

	[SerializeField]
	private BlueprintFactionReference m_OverrideFaction;

	[SerializeField]
	[AddInspector]
	[UsedImplicitly]
	private bool m_Dummy;

	[NonSerialized]
	private LogContext spawnLogContext;

	private new MyData Data => (MyData)base.Data;

	private new BaseUnitEntity SpawnedUnit => (BaseUnitEntity)base.SpawnedUnit;

	public bool IsControllingCompanion
	{
		get
		{
			BaseUnitEntity myCompanion = GetMyCompanion();
			if (myCompanion != null && myCompanion == SpawnedUnit)
			{
				return myCompanion.GetOptional<UnitPartCompanion>()?.GetCurrentSpawner() == this;
			}
			return false;
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MyData(this));
	}

	private BaseUnitEntity GetMyCompanion()
	{
		return Game.Instance.Player.AllCharacters.SingleItem((BaseUnitEntity u) => u.Blueprint.CheckEqualsWithPrototype(base.Blueprint));
	}

	public override void HandleAreaSpawnerInit()
	{
		spawnLogContext = LogContext.SceneInit;
		bool flag = ShouldControlUnit(GetMyCompanion());
		if (base.HasSpawned)
		{
			if (IsControllingCompanion && flag)
			{
				PlaceCompanion(SpawnedUnit, stayInGame: false);
			}
			else
			{
				ReleaseCompanion();
			}
		}
		if (flag)
		{
			base.HandleAreaSpawnerInit();
		}
	}

	protected override AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		spawnLogContext = LogContext.SpawnUnit;
		BaseUnitEntity myCompanion = GetMyCompanion();
		if (myCompanion == null)
		{
			LogSpawn("Companion is not recruited yet.");
			return null;
		}
		ClaimCompanion();
		return myCompanion;
	}

	private bool ShouldControlUnit(BaseUnitEntity unit)
	{
		bool flag = true;
		string msg = string.Empty;
		if (unit == null)
		{
			msg = "Companion is not recruited yet.";
			flag = false;
		}
		else if (Data == null || (!Data.DidSpawnOnce && !base.SpawnOnSceneInit))
		{
			msg = "Spawn was not triggered yet.  Just checking..";
			flag = false;
		}
		else
		{
			CompanionSpawner companionSpawner = unit.GetOptional<UnitPartCompanion>()?.GetCurrentSpawner();
			if (companionSpawner != null && companionSpawner != this && companionSpawner.ShouldControlUnit(unit))
			{
				msg = "Already controlled by spawner '" + companionSpawner.name + "'";
				flag = false;
			}
			else
			{
				CompanionState companionState = unit.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
				bool capitalPartyMode = Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
				if ((!m_SpawnWhenNone || companionState != 0) && (!m_SpawnWhenRemote || companionState != CompanionState.Remote) && (!m_SpawnWhenEx || companionState != CompanionState.ExCompanion) && (!(m_SpawnWhenInCapital && capitalPartyMode) || (companionState != CompanionState.Remote && companionState != CompanionState.InParty)))
				{
					msg = $"State is not a match. Companion state:{companionState}";
					flag = false;
				}
				else if (m_HideIfDead && unit.LifeState.IsFinallyDead)
				{
					msg = "Hidden as dead.";
					flag = false;
				}
				else
				{
					ConditionsHolder conditionsHolder = ShowCondition?.Get();
					if (conditionsHolder != null && !conditionsHolder.Check())
					{
						msg = "Show condition is false.";
						flag = false;
					}
					else
					{
						ConditionsHolder conditionsHolder2 = ControlCondition?.Get();
						if (conditionsHolder2 != null && !conditionsHolder2.Check())
						{
							msg = "Control condition is false.";
							flag = false;
						}
					}
				}
			}
		}
		if (!flag)
		{
			LogSpawn("Spawner failed to get companion control. Reason:");
			LogSpawn(msg);
		}
		return flag;
	}

	private void ClaimCompanion(bool stayInGame = false)
	{
		BaseUnitEntity myCompanion = GetMyCompanion();
		myCompanion.GetOptional<UnitPartCompanion>()?.SetSpawner(this);
		Data.SpawnedUnit = myCompanion;
		BlueprintFaction blueprintFaction = m_OverrideFaction?.Get();
		if (blueprintFaction != null)
		{
			myCompanion.Faction.Set(blueprintFaction);
		}
		PlaceCompanion(myCompanion, stayInGame);
	}

	public void ReleaseCompanion()
	{
		LogSpawn("Companion released.");
		Data.Clear();
	}

	private void UpdateState(bool stayInGame = false)
	{
		BaseUnitEntity myCompanion = GetMyCompanion();
		bool flag = ShouldControlUnit(myCompanion);
		bool isControllingCompanion = IsControllingCompanion;
		if (flag && !isControllingCompanion)
		{
			ClaimCompanion(stayInGame);
		}
		else if (!flag && isControllingCompanion)
		{
			ReleaseCompanion();
		}
	}

	private void PlaceCompanion(BaseUnitEntity unit, bool stayInGame)
	{
		bool flag = false;
		if (ShouldControlUnit(unit))
		{
			unit.IsInGame = true;
			unit.Position = base.ViewTransform.position;
			unit.SetOrientation(base.ViewTransform.rotation.eulerAngles.y);
			if ((bool)unit.View)
			{
				unit.View.ViewTransform.position = base.ViewTransform.position;
			}
			flag = true;
			unit.GetOptional<UnitPartPetOwner>()?.PlacePet();
		}
		else if (!stayInGame)
		{
			UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
			if (optional == null || !optional.IsControllableInParty())
			{
				unit.IsInGame = false;
			}
		}
		LogSpawn(flag ? "Spawn Success!" : "Spawn failed.");
		Data.OnPlaceCompanion(flag);
	}

	void IPartyHandler.HandleAddCompanion()
	{
		spawnLogContext = LogContext.AddedToParty;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IPartyHandler.HandleCompanionActivated()
	{
		spawnLogContext = LogContext.Activated;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IPartyHandler.HandleCompanionRemoved(bool stayInGame)
	{
		spawnLogContext = LogContext.RemovedFromParty;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState(stayInGame);
		}
	}

	void IPartyHandler.HandleCapitalModeChanged()
	{
		spawnLogContext = LogContext.CapitalModeChanged;
		UpdateState();
	}

	void ICompanionChangeHandler.HandleRecruit()
	{
		spawnLogContext = LogContext.Recruited;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void ICompanionChangeHandler.HandleUnrecruit()
	{
		spawnLogContext = LogContext.Unrecruited;
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IEtudesUpdateHandler.OnEtudesUpdate()
	{
		spawnLogContext = LogContext.EtudeUpdate;
		UpdateState();
	}

	private void LogSpawn(string msg)
	{
	}
}
