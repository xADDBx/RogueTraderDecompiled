using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Overtips.Console;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.Console;

public class OvertipTransitionConsoleView : OvertipTransitionView
{
	[Header("Console")]
	[SerializeField]
	private OvertipConsoleView m_OvertipConsoleView;

	[SerializeField]
	private bool m_NeedHintPositionCorrection;

	[SerializeField]
	[ConditionalShow("m_NeedHintPositionCorrection")]
	private float m_UpperY;

	[SerializeField]
	[ConditionalShow("m_NeedHintPositionCorrection")]
	private float m_LowerY;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.MapObjectIsHighlited.Subscribe(delegate(bool value)
		{
			if (value)
			{
				m_Button.OnPointerEnter();
			}
			else
			{
				m_Button.OnPointerExit();
			}
		}));
		if (m_NeedHintPositionCorrection)
		{
			m_OvertipConsoleView.SetConfirmPosition(string.IsNullOrEmpty(base.ViewModel.Title.Value) ? m_UpperY : m_LowerY);
		}
		m_OvertipConsoleView.SetConfirmHint(base.ViewModel.MapObjectIsHighlited, UIStrings.Instance.ActionTexts.ExitArea);
		AddDisposable(base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
		}));
		AddDisposable(m_OvertipConsoleView);
	}
}
