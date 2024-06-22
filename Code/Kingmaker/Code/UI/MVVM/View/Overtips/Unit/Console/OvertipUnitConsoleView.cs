using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Overtips.Console;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.Console;

public class OvertipUnitConsoleView : OvertipUnitView
{
	[Header("Console")]
	[SerializeField]
	private OvertipConsoleView m_OvertipConsoleView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_OvertipConsoleView.SetConfirmHint(base.ViewModel.UnitState.NeedConsoleHint, UIStrings.Instance.ActionTexts.Talk);
		AddDisposable(base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			if (!Game.Instance.Player.IsInCombat)
			{
				m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
			}
		}));
		AddDisposable(Visibility.Subscribe(delegate(UnitOvertipVisibility value)
		{
			if (Game.Instance.Player.IsInCombat)
			{
				List<BaseUnitEntity> list = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity unit) => unit.IsInCombat && unit.Faction.IsPlayerEnemy && unit.IsVisibleForPlayer && !unit.LifeState.IsDead).ToList();
				m_OvertipConsoleView.SetPaginator(list.Count > 1 && list.Contains(base.ViewModel.Unit) && Game.Instance.TurnController.TurnBasedModeActive && Game.Instance.TurnController.IsPlayerTurn, value == UnitOvertipVisibility.Maximized || value == UnitOvertipVisibility.Full);
			}
		}));
		AddDisposable(m_OvertipConsoleView);
	}
}
