using Kingmaker.Code.UI.MVVM.View.Overtips.Console;
using Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.OvertipParts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.Console;

public class OvertipMapObjectTwitchDropsConsoleView : OvertipMapObjectTwitchDropsView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Console")]
	[SerializeField]
	private OvertipConsoleView m_OvertipConsoleView;

	[SerializeField]
	private MapObjectOvertipNameBlockView m_OvertipNameBlockView;

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
		m_OvertipNameBlockView.Bind(base.ViewModel);
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_OvertipConsoleView.SetConfirmHint(base.ViewModel.MapObjectIsHighlited, value);
		}));
		AddDisposable(base.ViewModel.MapObjectIsHighlited.Or(base.ViewModel.IsMouseOverUI).Subscribe(delegate(bool value)
		{
			SetHighlightImage(value);
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
			bool flag = string.IsNullOrEmpty(base.ViewModel.Name.Value);
			m_OvertipConsoleView.SetConfirmPosition(flag ? m_ConfirmUpperY : m_ConfirmLowerY);
			bool flag2 = string.IsNullOrEmpty(base.ViewModel.ObjectDescription.Value) && string.IsNullOrEmpty(base.ViewModel.ObjectSkillCheckText.Value);
			m_OvertipConsoleView.SetPaginatorPosition(flag2 ? m_PaginatorLowerY : m_PaginatorUpperY);
		}
		AddDisposable(base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
		}));
		AddDisposable(m_OvertipConsoleView);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (base.IsBinded)
		{
			base.ViewModel.MapObjectEntity.View.Highlighted = true;
			base.ViewModel.IsMouseOverUI.Value = true;
			Game.Instance.CursorController.SetMapObjectCursor(base.ViewModel.MapObjectEntity.View, isHighlighted: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.IsBinded)
		{
			base.ViewModel.MapObjectEntity.View.Highlighted = false;
			base.ViewModel.IsMouseOverUI.Value = false;
			Game.Instance.CursorController.SetMapObjectCursor(base.ViewModel.MapObjectEntity.View, isHighlighted: false);
		}
	}
}
