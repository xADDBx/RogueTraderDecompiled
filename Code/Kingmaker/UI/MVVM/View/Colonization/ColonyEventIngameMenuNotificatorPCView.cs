using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Colonization.Events;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization;

public class ColonyEventIngameMenuNotificatorPCView : ViewBase<ColonyEventIngameMenuNotificatorVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private Image m_Icon;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.HasEvent.Subscribe(SetVisibility));
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite val)
		{
			m_Icon.sprite = val;
		}));
		AddDisposable(m_Icon.SetHint(UIStrings.Instance.ColonyEventsTexts.NeedsVisitMechanicString.Text));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetVisibility(bool hasEvent)
	{
		m_CanvasGroup.alpha = (hasEvent ? 1f : 0f);
		m_CanvasGroup.interactable = hasEvent;
		m_CanvasGroup.blocksRaycasts = hasEvent;
	}
}
