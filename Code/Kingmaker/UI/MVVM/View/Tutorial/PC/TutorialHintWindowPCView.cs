using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tutorial.PC;

public class TutorialHintWindowPCView : TutorialWindowPCView<TutorialHintWindowVM>
{
	[Space]
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private int m_ViewPortHeight = 350;

	[SerializeField]
	private LayoutElement m_ViewPort;

	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private OwlcatButton m_EncyclopediaButton;

	protected override bool IsBigTutorial => false;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ConfirmButtonText.text = UIStrings.Instance.Tutorial.GotIt.Text;
		AddDisposable(m_ConfirmButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Hide();
		}));
		bool isPossibleGoToEncyclopedia = UIUtility.EntityLinkActions.IsPossibleGoToEncyclopedia;
		bool value = base.ViewModel.EncyclopediaLinkExist.Value;
		m_EncyclopediaButton.gameObject.SetActive(isPossibleGoToEncyclopedia && value);
		if (isPossibleGoToEncyclopedia && value)
		{
			AddDisposable(m_EncyclopediaButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.GoToEncyclopedia));
			AddDisposable(m_EncyclopediaButton.SetHint(UIStrings.Instance.Tutorial.Encyclopedia.Text));
		}
		SetContent();
		m_ScrollRect.ScrollToTop();
	}

	protected override void OnShow()
	{
		base.OnShow();
		UISounds.Instance.Sounds.Tutorial.ShowSmallTutorial.Play();
		StartCoroutine(SetSizeDelayed());
	}

	protected override void OnHide()
	{
		base.OnHide();
		UISounds.Instance.Sounds.Tutorial.HideSmallTutorial.Play();
	}

	private void SetContent()
	{
		SetPage(base.ViewModel.Pages.FirstOrDefault());
	}

	private void SetWindowSize()
	{
		m_ViewPort.preferredHeight = Mathf.Min(m_ViewPortHeight, m_Content.sizeDelta.y);
	}

	private IEnumerator SetSizeDelayed()
	{
		yield return new WaitForEndOfFrame();
		SetWindowSize();
	}
}
