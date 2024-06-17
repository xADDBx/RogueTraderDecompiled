using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Formation.Base;

public class FormationCharacterBaseView : ViewBase<FormationCharacterVM>
{
	[SerializeField]
	[UsedImplicitly]
	protected OwlcatButton m_Button;

	[SerializeField]
	[UsedImplicitly]
	private Image m_Portrait;

	[SerializeField]
	[UsedImplicitly]
	private Color m_GreyColor;

	[SerializeField]
	[UsedImplicitly]
	protected FormationCharacterDragComponent m_FormationCharacterDragComponent;

	protected override void BindViewImplementation()
	{
		m_FormationCharacterDragComponent.Initialize(base.transform.parent as RectTransform);
		m_Portrait.sprite = base.ViewModel.PortraitSprite;
		SetupPosition();
		AddDisposable(base.ViewModel.FormationUpdated.Subscribe(delegate
		{
			SetupPosition();
		}));
		AddDisposable(base.ViewModel.IsInteractable.Subscribe(SetInteractable));
		UILog.ViewBinded("FormationCharacterPCView");
	}

	protected virtual void SetupPosition()
	{
		Vector3 localPosition = base.ViewModel.GetLocalPosition();
		localPosition.x -= localPosition.x % 23f;
		localPosition.y -= localPosition.y % 23f;
		base.transform.localPosition = localPosition;
		if (!(base.ViewModel.GetLocalPosition() == localPosition))
		{
			base.ViewModel.MoveCharacter(((Vector2)localPosition - base.ViewModel.OffsetPosition) / 40f);
		}
	}

	private void SetInteractable(bool value)
	{
		m_Button.Interactable = value;
		m_FormationCharacterDragComponent.IsInteractable = value;
		m_Portrait.color = (value ? Color.white : m_GreyColor);
	}

	protected override void DestroyViewImplementation()
	{
		base.ViewModel.MoveCharacter(((Vector2)base.transform.localPosition - base.ViewModel.OffsetPosition) / 40f);
		UILog.ViewUnbinded("FormationCharacterView");
	}
}
