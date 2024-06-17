using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tutorial.PC;

public class TutorialModalWindowPCView : TutorialWindowPCView<TutorialModalWindowVM>
{
	[Space]
	[SerializeField]
	private GameObject m_EncyclopediaBlock;

	[SerializeField]
	private OwlcatButton m_EncyclopediaButton;

	[SerializeField]
	private TextMeshProUGUI m_EncyclopediaButtonText;

	[SerializeField]
	private GameObject m_PagesBlock;

	[SerializeField]
	private PageNavigationPC m_PageNavigation;

	[SerializeField]
	private TextMeshProUGUI m_PageNavigationText;

	[SerializeField]
	private float m_EncyclopediaButtonDefaultSize = 18f;

	protected override bool IsShowDefaultSprite => true;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_EncyclopediaButtonText.text = UIStrings.Instance.Tutorial.Encyclopedia.Text;
		AddDisposable(base.ViewModel.CurrentPage.Subscribe(base.SetPage));
		bool isPossibleGoToEncyclopedia = UIUtility.EntityLinkActions.IsPossibleGoToEncyclopedia;
		bool value = base.ViewModel.EncyclopediaLinkExist.Value;
		m_EncyclopediaBlock.SetActive(isPossibleGoToEncyclopedia && value);
		if (isPossibleGoToEncyclopedia && value)
		{
			AddDisposable(m_EncyclopediaButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.GoToEncyclopedia));
		}
		m_PagesBlock.SetActive(base.ViewModel.MultiplePages);
		m_PageNavigation.Initialize(base.ViewModel.PageCount, base.ViewModel.CurrentPageIndex);
		AddDisposable(base.ViewModel.CurrentPageIndex.Subscribe(delegate
		{
			m_PageNavigationText.text = base.ViewModel.CurrentPageIndex.Value + 1 + "/" + base.ViewModel.PageCount;
			m_ConfirmButtonText.text = ((base.ViewModel.CurrentPageIndex.Value + 1 < base.ViewModel.PageCount) ? UIStrings.Instance.Tutorial.Next.Text : UIStrings.Instance.Tutorial.Complete.Text);
		}));
		AddDisposable(m_ConfirmButton.OnLeftClickAsObservable().Subscribe(OnNext));
		AddDisposable(m_PageNavigation);
		TooltipHelper.HideTooltip();
	}

	private void OnNext()
	{
		if (base.ViewModel.CurrentPageIndex.Value + 1 < base.ViewModel.PageCount)
		{
			base.ViewModel.CurrentPageIndex.Value++;
			UISounds.Instance.Sounds.Tutorial.ChangeTutorialPage.Play();
		}
		else
		{
			base.ViewModel.Hide();
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		UISounds.Instance.Sounds.Tutorial.ShowBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		UISounds.Instance.Sounds.Tutorial.HideBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: false);
	}

	protected override void SetTextsSize(float multiplier)
	{
		m_EncyclopediaButtonText.fontSize = m_EncyclopediaButtonDefaultSize * multiplier;
		base.SetTextsSize(multiplier);
	}
}
