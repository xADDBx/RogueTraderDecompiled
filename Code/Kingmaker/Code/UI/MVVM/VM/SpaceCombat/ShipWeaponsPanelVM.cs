using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat;

public class ShipWeaponsPanelVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IClickMechanicActionBarSlotHandler, ISubscriber, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IStarshipLevelUpHandler, ISubscriber<IStarshipEntity>, IInterruptTurnStartHandler, IUnitCommandStartHandler, IWarhammerAttackHandler, IUnitCommandActHandler, IUnitCommandEndHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IDeliverAbilityEffectHandler, IUnitDirectHoverUIHandler
{
	public readonly ReactiveProperty<bool> IsPlayerTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsTorpedoesTurn = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<UnitEntityView> HighlightedUnit = new ReactiveProperty<UnitEntityView>();

	public readonly Dictionary<WeaponSlotType, AbilitiesGroupVM> WeaponAbilitiesGroups;

	public readonly AbilitiesGroupVM AbilitiesGroup;

	public readonly BoolReactiveProperty IsActive = new BoolReactiveProperty();

	private bool m_SlotsUpdateQueued;

	private BaseUnitEntity Unit => Game.Instance.SelectionCharacter.SingleSelectedUnit.Value;

	public ShipWeaponsPanelVM()
	{
		UISpaceCombatTexts spaceCombatTexts = UIStrings.Instance.SpaceCombatTexts;
		WeaponAbilitiesGroups = new Dictionary<WeaponSlotType, AbilitiesGroupVM>
		{
			{
				WeaponSlotType.Port,
				new AbilitiesGroupVM(spaceCombatTexts.PortAbilitiesGroupLabel)
			},
			{
				WeaponSlotType.Prow,
				new AbilitiesGroupVM(spaceCombatTexts.ProwAbilitiesGroupLabel)
			},
			{
				WeaponSlotType.Dorsal,
				new AbilitiesGroupVM(spaceCombatTexts.DorsalAbilitiesGroupLabel)
			},
			{
				WeaponSlotType.Starboard,
				new AbilitiesGroupVM(spaceCombatTexts.StarboardAbilitiesGroupLabel)
			}
		};
		AddDisposable(AbilitiesGroup = new AbilitiesGroupVM(string.Empty));
		AddDisposable(Game.Instance.SelectionCharacter.SingleSelectedUnit.Subscribe(delegate
		{
			OnUnitChanged();
		}));
		AddDisposable(EventBus.Subscribe(this));
		HandleTurnBasedModeSwitched(TurnController.IsInTurnBasedCombat());
	}

	protected override void DisposeImplementation()
	{
		ClearWeaponAbilities();
	}

	private void OnUnitChanged()
	{
		if (Unit == null)
		{
			foreach (KeyValuePair<WeaponSlotType, AbilitiesGroupVM> weaponAbilitiesGroup in WeaponAbilitiesGroups)
			{
				weaponAbilitiesGroup.Deconstruct(out var _, out var value);
				value.SetAbilities(null, null);
			}
			AbilitiesGroup.SetAbilities(null, null);
		}
		else
		{
			UpdateWeaponAbilities();
			UpdateAbilities();
		}
	}

	public void UpdateWeaponAbilities()
	{
		if (!Unit.IsStarship())
		{
			return;
		}
		Dictionary<WeaponSlotType, List<WeaponSlot>> dictionary = new Dictionary<WeaponSlotType, List<WeaponSlot>>();
		foreach (WeaponSlot weaponSlot in Unit.GetHull().WeaponSlots)
		{
			Ability ability = weaponSlot?.ActiveAbility;
			if (ability != null && !ability.Hidden && !ability.Blueprint.IsCantrip)
			{
				if (!dictionary.ContainsKey(weaponSlot.Type))
				{
					dictionary[weaponSlot.Type] = new List<WeaponSlot>();
				}
				dictionary[weaponSlot.Type].Add(weaponSlot);
			}
		}
		foreach (var (weaponSlotType2, abilitiesGroupVM2) in WeaponAbilitiesGroups)
		{
			dictionary.TryGetValue(weaponSlotType2, out var value);
			abilitiesGroupVM2.SetWeaponSlotAbilities(value, Unit, weaponSlotType2);
		}
	}

	private void UpdateAbilities()
	{
		if (!Unit.IsStarship())
		{
			return;
		}
		List<Ability> list = new List<Ability>();
		foreach (Ability ability in Unit.Abilities)
		{
			if (!ability.Hidden && !ability.Blueprint.IsCantrip && ability.SourceItem == null)
			{
				list.Add(ability);
			}
		}
		AbilitiesGroup.SetAbilities(list, Unit);
	}

	private void UpdateSlots(bool onTurnStart = false)
	{
		if (m_SlotsUpdateQueued)
		{
			return;
		}
		m_SlotsUpdateQueued = true;
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			m_SlotsUpdateQueued = false;
			foreach (KeyValuePair<WeaponSlotType, AbilitiesGroupVM> weaponAbilitiesGroup in WeaponAbilitiesGroups)
			{
				UpdateFunc(weaponAbilitiesGroup.Value.Slots);
			}
			UpdateFunc(AbilitiesGroup.Slots);
		});
		void UpdateFunc(IList<ActionBarSlotVM> slots)
		{
			foreach (ActionBarSlotVM slot in slots)
			{
				slot.UpdateResources();
				if (onTurnStart)
				{
					slot.CloseConvertsOnTurnStart();
				}
			}
		}
	}

	private void ClearWeaponAbilities()
	{
		WeaponAbilitiesGroups.Values.ForEach(delegate(AbilitiesGroupVM groupVm)
		{
			groupVm.Dispose();
		});
		WeaponAbilitiesGroups.Clear();
	}

	private void UpdatePlayerTurn(bool isTurnBased)
	{
		TurnController turnController = Game.Instance.TurnController;
		IsPlayerTurn.Value = isTurnBased && turnController.IsPlayerTurn;
		IsTorpedoesTurn.Value = isTurnBased && turnController.IsPlayerTurn && turnController.CurrentUnit != Game.Instance.Player.PlayerShip;
	}

	public void Deactivate()
	{
		IsActive.Value = false;
	}

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		Deactivate();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdatePlayerTurn(isTurnBased);
		UpdateSlots();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdatePlayerTurn(isTurnBased);
		UpdateSlots();
	}

	public void HandleStarshipLevelUp(int newLevel, LevelUpManager manager)
	{
		UpdateSlots();
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdatePlayerTurn(isTurnBased: true);
		UpdateSlots();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		UpdateSlots();
	}

	public void OnDeliverAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
	{
		UpdateSlots();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		HighlightedUnit.Value = (isHover ? (unitEntityView as UnitEntityView) : null);
	}
}
