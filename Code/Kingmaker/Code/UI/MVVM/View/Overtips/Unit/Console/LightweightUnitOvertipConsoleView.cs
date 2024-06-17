using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Overtips.Console;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.Console;

public class LightweightUnitOvertipConsoleView : LightweightUnitOvertipView
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
			m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
		}));
		AddDisposable(m_OvertipConsoleView);
	}
}
