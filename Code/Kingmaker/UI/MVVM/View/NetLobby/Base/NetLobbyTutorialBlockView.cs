using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.NetLobby.Base;

public class NetLobbyTutorialBlockView : ViewBase<NetLobbyTutorialBlockVM>, IWidgetView
{
	[SerializeField]
	private Image m_BlockImage;

	[SerializeField]
	private TextMeshProUGUI m_BlockDescription;

	[SerializeField]
	private FadeAnimator m_BlockFadeAnimator;

	[SerializeField]
	private RectTransform m_RightArrowImage;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_BlockFadeAnimator.Initialize();
		m_BlockImage.sprite = base.ViewModel.BlockSprite;
		m_BlockDescription.text = base.ViewModel.BlockDescription;
		if (m_BlockFadeAnimator.CanvasGroup != null)
		{
			m_BlockFadeAnimator.CanvasGroup.alpha = 0f;
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void ScrollToTop()
	{
		m_ScrollRectExtended.ScrollToTop();
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRectExtended.scrollSensitivity);
		m_ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	public void ShowAnimation(bool withAnimation = false)
	{
		if (!withAnimation && m_BlockFadeAnimator.CanvasGroup != null)
		{
			m_BlockFadeAnimator.CanvasGroup.alpha = 1f;
			m_BlockFadeAnimator.CanvasGroup.blocksRaycasts = true;
		}
		else
		{
			m_BlockFadeAnimator.AppearAnimation();
		}
	}

	public void SetRightArrowVisible(bool state)
	{
		m_RightArrowImage.gameObject.SetActive(state);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((NetLobbyTutorialBlockVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is NetLobbyTutorialBlockVM;
	}
}
