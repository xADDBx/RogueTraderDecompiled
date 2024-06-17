using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Overtips.Console;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.Console;

public class OvertipMapObjectSimpleConsoleView : OvertipMapObjectSimpleView
{
	[Header("Console")]
	[SerializeField]
	private OvertipConsoleView m_OvertipConsoleView;

	[SerializeField]
	private bool m_NeedHintPositionCorrection;

	[SerializeField]
	[ConditionalShow("m_NeedHintPositionCorrection")]
	private float m_ConfirmUpperY;

	[SerializeField]
	[ConditionalShow("m_NeedHintPositionCorrection")]
	private float m_ConfirmLowerY;

	[SerializeField]
	[ConditionalShow("m_NeedHintPositionCorrection")]
	private float m_PaginatorUpperY;

	[SerializeField]
	[ConditionalShow("m_NeedHintPositionCorrection")]
	private float m_PaginatorLowerY;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_NeedHintPositionCorrection)
		{
			bool flag = string.IsNullOrEmpty(base.ViewModel.Name.Value);
			m_OvertipConsoleView.SetConfirmPosition(flag ? m_ConfirmUpperY : m_ConfirmLowerY);
			bool flag2 = string.IsNullOrEmpty(base.ViewModel.ObjectDescription.Value) || string.IsNullOrEmpty(base.ViewModel.ObjectSkillCheckText.Value);
			m_OvertipConsoleView.SetPaginatorPosition(flag2 ? m_PaginatorLowerY : m_PaginatorUpperY);
		}
		m_OvertipConsoleView.SetConfirmHint(base.ViewModel.MapObjectIsHighlited, UIStrings.Instance.ActionTexts.Interact);
		AddDisposable(base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
		}));
		AddDisposable(m_OvertipConsoleView);
	}
}
