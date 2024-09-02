using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Overtips.Console;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.Console;

public class OvertipMapObjectInteractionConsoleView : OvertipMapObjectInteractionView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
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
		m_OvertipConsoleView.SetConfirmHint(base.ViewModel.MapObjectIsHighlited, GetHintLabel(base.ViewModel.Type));
		AddDisposable(base.ViewModel.HasSurrounding.CombineLatest(base.ViewModel.IsChosen, (bool hasSur, bool chosen) => new { hasSur, chosen }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			m_OvertipConsoleView.SetPaginator(value.hasSur, value.chosen);
		}));
		AddDisposable(m_OvertipConsoleView);
	}

	private string GetHintLabel(UIInteractionType type)
	{
		UIActionText actionTexts = UIStrings.Instance.ActionTexts;
		switch (type)
		{
		case UIInteractionType.None:
			return string.Empty;
		case UIInteractionType.Action:
		case UIInteractionType.Credits:
			return actionTexts.Interact;
		case UIInteractionType.Move:
			return actionTexts.Move;
		case UIInteractionType.Info:
			return actionTexts.Inspect;
		default:
			return string.Empty;
		}
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
