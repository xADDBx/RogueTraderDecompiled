using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UniRx;

namespace Kingmaker.Controllers;

public class SelectionCharacterController : IControllerStart, IController, IControllerEnable, IControllerTick, IControllerStop, IControllerReset, IFullScreenUIHandler, ISubscriber, IFullScreenUIHandlerWorkaround, IPartyHandler, ISubscriber<IBaseUnitEntity>
{
	public readonly ReactiveProperty<BaseUnitEntity> SelectedUnit = new ReactiveProperty<BaseUnitEntity>();

	public readonly ReactiveProperty<BaseUnitEntity> SelectedUnitInUI = new ReactiveProperty<BaseUnitEntity>();

	public readonly ReactiveProperty<bool> IsSingleSelected = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<BaseUnitEntity> SingleSelectedUnit = new ReactiveProperty<BaseUnitEntity>();

	public ReactiveCommand ActualGroupUpdated = new ReactiveCommand();

	private FullScreenUIType m_FullScreenType;

	private BaseUnitEntity m_FullScreenSelectedUnit;

	private bool m_NeedUpdate;

	private readonly List<BaseUnitEntity> m_ActualGroup = new List<BaseUnitEntity>();

	private bool m_FullScreenState;

	private CompositeDisposable m_SelectedUnitsSubscription;

	private bool m_ControllerStarted;

	public ReactiveCollection<BaseUnitEntity> SelectedUnits { get; } = new ReactiveCollection<BaseUnitEntity>();


	public BaseUnitEntity FirstSelectedUnit => SelectedUnits.FirstOrDefault();

	public bool ReorderEnable => !m_FullScreenState;

	public List<BaseUnitEntity> ActualGroup
	{
		get
		{
			if (m_ControllerStarted && m_ActualGroup.Count == 0)
			{
				UpdateSelectedUnits();
			}
			return m_ActualGroup;
		}
	}

	public static bool CanSelectUnit
	{
		get
		{
			if (!Game.Instance.TurnController.IsPreparationTurn)
			{
				return !TurnController.IsInTurnBasedCombat();
			}
			return true;
		}
	}

	private bool WithRemote
	{
		get
		{
			if (RootUIContext.Instance.IsSpace)
			{
				return true;
			}
			if (m_FullScreenState)
			{
				AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
				if (loadedAreaState != null && loadedAreaState.Settings.CapitalPartyMode)
				{
					FullScreenUIType fullScreenType = m_FullScreenType;
					if ((uint)(fullScreenType - 4) <= 1u || fullScreenType == FullScreenUIType.Vendor)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public void OnEnable()
	{
	}

	public void OnStart()
	{
		m_NeedUpdate = true;
		UIAccess.SelectionManager.Or(null)?.SelectAll(Game.Instance.Player.PartyCharacters.Select((UnitReference r) => r.Entity.ToBaseUnitEntity()));
		if (SelectedUnits.Count == 0)
		{
			UIAccess.SelectionManager.Or(null)?.SelectAll();
		}
		UpdateSelectedUnits();
		m_SelectedUnitsSubscription?.Dispose();
		m_SelectedUnitsSubscription = new CompositeDisposable();
		m_SelectedUnitsSubscription.Add(SelectedUnits.ObserveAdd().Subscribe(delegate
		{
			m_NeedUpdate = true;
		}));
		m_SelectedUnitsSubscription.Add(SelectedUnits.ObserveRemove().Subscribe(delegate
		{
			m_NeedUpdate = true;
		}));
		m_SelectedUnitsSubscription.Add(SelectedUnits.ObserveReplace().Subscribe(delegate
		{
			m_NeedUpdate = true;
		}));
		m_SelectedUnitsSubscription.Add(SelectedUnits.ObserveMove().Subscribe(delegate
		{
			m_NeedUpdate = true;
		}));
		m_SelectedUnitsSubscription.Add(SelectedUnits.ObserveReset().Subscribe(delegate
		{
			m_NeedUpdate = true;
		}));
		m_ControllerStarted = true;
	}

	public void OnStop()
	{
		m_FullScreenState = false;
		m_FullScreenType = FullScreenUIType.Unknown;
		m_FullScreenSelectedUnit = null;
		m_NeedUpdate = false;
		SelectedUnit.Value = null;
		SelectedUnitInUI.Value = null;
		SingleSelectedUnit.Value = null;
		m_ActualGroup.Clear();
		m_SelectedUnitsSubscription?.Dispose();
		m_SelectedUnitsSubscription = null;
		foreach (BaseUnitEntity item in SelectedUnits.ToTempList())
		{
			UIAccess.SelectionManager.Or(null)?.UnselectUnit(item);
		}
		SelectedUnits.Clear();
		m_ControllerStarted = false;
	}

	public void OnReset()
	{
		UpdateSelectedUnits();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (m_NeedUpdate)
		{
			m_NeedUpdate = false;
			UpdateSelectedUnits();
			ActualGroupUpdated.Execute();
		}
	}

	public bool IsSelected(BaseUnitEntity unit)
	{
		if (m_FullScreenState)
		{
			return m_FullScreenSelectedUnit == unit;
		}
		return SelectedUnits.Contains(unit);
	}

	public void SetSelected(BaseUnitEntity unit, bool force = false, bool forceFullScreenState = false)
	{
		if (m_FullScreenState || forceFullScreenState)
		{
			m_FullScreenSelectedUnit = unit;
			m_NeedUpdate = true;
			return;
		}
		if (UIAccess.SelectionManager != null)
		{
			UIAccess.SelectionManager.SwitchSelectionUnitInGroup(unit, canAddToSelection: true, force);
		}
		else if (Game.Instance.IsControllerGamepad && Game.Instance.Player.IsInCombat)
		{
			SelectedUnits.ForEach(delegate(BaseUnitEntity u)
			{
				u.IsSelected = false;
			});
			SelectedUnits.Clear();
			SelectedUnits.Add(unit);
			unit.IsSelected = true;
		}
		m_NeedUpdate = true;
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType type)
	{
		m_FullScreenState = state;
		HandleFullScreenUiChangedInternal(state, type);
	}

	public void HandleFullScreenUiChangedWorkaround(bool state, FullScreenUIType type)
	{
		HandleFullScreenUiChangedInternal(state, type);
	}

	private void HandleFullScreenUiChangedInternal(bool state, FullScreenUIType type)
	{
		m_FullScreenState = state;
		m_FullScreenType = type;
		if (state)
		{
			if (m_FullScreenSelectedUnit == null)
			{
				m_FullScreenSelectedUnit = SelectedUnit.Value ?? FirstSelectedUnit;
			}
			if (!(m_FullScreenSelectedUnit is UnitEntity))
			{
				m_FullScreenSelectedUnit = Game.Instance.Player?.MainCharacterEntity;
			}
		}
		else
		{
			m_FullScreenSelectedUnit = null;
		}
		m_NeedUpdate = true;
	}

	private void UpdateSelectedUnits()
	{
		bool withRemote = WithRemote;
		UIUtility.GetGroup(m_ActualGroup, withRemote, withRemote);
		IsSingleSelected.Value = Game.Instance.IsControllerGamepad || SelectedUnits.Count == 1;
		if (IsSingleSelected.Value)
		{
			SingleSelectedUnit.Value = (Game.Instance.IsControllerMouse ? FirstSelectedUnit : SelectedUnit.Value);
		}
		else
		{
			SingleSelectedUnit.Value = null;
		}
		BaseUnitEntity value = (m_FullScreenState ? m_FullScreenSelectedUnit : (SelectedUnit.Value ?? FirstSelectedUnit));
		SelectedUnitInUI.Value = value;
	}

	public void HandleAddCompanion()
	{
		m_NeedUpdate = true;
	}

	public void HandleCompanionActivated()
	{
		m_NeedUpdate = true;
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
		m_NeedUpdate = true;
	}

	public void HandleCapitalModeChanged()
	{
		m_NeedUpdate = true;
	}

	public void SwitchCharacter(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		if (unit1?.Master == null && m_ActualGroup.Contains(unit1) && unit2?.Master == null && m_ActualGroup.Contains(unit2))
		{
			int num = Game.Instance.Player.PartyCharacters.IndexOf(unit1.FromBaseUnitEntity());
			int num2 = Game.Instance.Player.PartyCharacters.IndexOf(unit2.FromBaseUnitEntity());
			List<UnitReference> partyCharacters = Game.Instance.Player.PartyCharacters;
			int index = num;
			List<UnitReference> partyCharacters2 = Game.Instance.Player.PartyCharacters;
			int index2 = num2;
			UnitReference unitReference = Game.Instance.Player.PartyCharacters[num2];
			UnitReference unitReference2 = Game.Instance.Player.PartyCharacters[num];
			UnitReference unitReference4 = (partyCharacters[index] = unitReference);
			unitReference4 = (partyCharacters2[index2] = unitReference2);
			Game.Instance.Player.InvalidateCharacterLists();
			m_NeedUpdate = true;
			EventBus.RaiseEvent(delegate(ISwitchPartyCharactersHandler h)
			{
				h.HandleSwitchPartyCharacters(unit1, unit2);
			});
		}
	}

	public void ReselectCurrentUnit()
	{
		IsSingleSelected.Value = Game.Instance.IsControllerGamepad || SelectedUnits.Count == 1;
		if (IsSingleSelected.Value)
		{
			SingleSelectedUnit.SetValueAndForceNotify(Game.Instance.IsControllerMouse ? FirstSelectedUnit : SelectedUnit.Value);
		}
		else
		{
			SingleSelectedUnit.Value = null;
		}
	}
}
