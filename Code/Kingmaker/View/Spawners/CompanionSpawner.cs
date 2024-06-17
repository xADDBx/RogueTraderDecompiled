using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
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

public class CompanionSpawner : UnitSpawnerBase, IPartyHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IAddInspectorGUI, ICompanionChangeHandler, IEtudesUpdateHandler
{
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

	private new MyData Data => (MyData)base.Data;

	private new BaseUnitEntity SpawnedUnit => (BaseUnitEntity)base.SpawnedUnit;

	private bool IsControllingCompanion
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
		BaseUnitEntity myCompanion = GetMyCompanion();
		if (myCompanion == null)
		{
			return null;
		}
		ClaimCompanion();
		return myCompanion;
	}

	private bool ShouldControlUnit(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return false;
		}
		if (Data == null || (!Data.DidSpawnOnce && !base.SpawnOnSceneInit))
		{
			return false;
		}
		CompanionSpawner companionSpawner = unit.GetOptional<UnitPartCompanion>()?.GetCurrentSpawner();
		if (companionSpawner != null && companionSpawner != this && companionSpawner.ShouldControlUnit(unit))
		{
			return false;
		}
		CompanionState companionState = unit.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
		bool capitalPartyMode = Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
		if (((m_SpawnWhenNone && companionState == CompanionState.None) || (m_SpawnWhenRemote && companionState == CompanionState.Remote) || (m_SpawnWhenEx && companionState == CompanionState.ExCompanion) || (m_SpawnWhenInCapital && capitalPartyMode && (companionState == CompanionState.Remote || companionState == CompanionState.InParty))) && ShouldShowUnit(unit))
		{
			return (ControlCondition?.Get())?.Check() ?? true;
		}
		return false;
	}

	private bool ShouldShowUnit(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return false;
		}
		if (!m_HideIfDead || !unit.LifeState.IsFinallyDead)
		{
			return (ShowCondition?.Get())?.Check() ?? true;
		}
		return false;
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
		bool didPlace = false;
		if (ShouldControlUnit(unit))
		{
			unit.IsInGame = true;
			unit.Position = base.ViewTransform.position;
			unit.SetOrientation(base.ViewTransform.rotation.eulerAngles.y);
			if ((bool)unit.View)
			{
				unit.View.ViewTransform.position = base.ViewTransform.position;
			}
			didPlace = true;
		}
		else if (!stayInGame)
		{
			UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
			if (optional == null || !optional.IsControllableInParty())
			{
				unit.IsInGame = false;
			}
		}
		Data.OnPlaceCompanion(didPlace);
	}

	void IPartyHandler.HandleAddCompanion()
	{
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IPartyHandler.HandleCompanionActivated()
	{
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IPartyHandler.HandleCompanionRemoved(bool stayInGame)
	{
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState(stayInGame);
		}
	}

	void IPartyHandler.HandleCapitalModeChanged()
	{
		UpdateState();
	}

	void ICompanionChangeHandler.HandleRecruit()
	{
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void ICompanionChangeHandler.HandleUnrecruit()
	{
		if (EventInvokerExtensions.BaseUnitEntity.Blueprint.CheckEqualsWithPrototype(base.Blueprint))
		{
			UpdateState();
		}
	}

	void IEtudesUpdateHandler.OnEtudesUpdate()
	{
		UpdateState();
	}
}
