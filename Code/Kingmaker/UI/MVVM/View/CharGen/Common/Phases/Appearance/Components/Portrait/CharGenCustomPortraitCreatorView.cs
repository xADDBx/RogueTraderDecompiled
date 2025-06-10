using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.CharGen.Common.Portrait;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public class CharGenCustomPortraitCreatorView : ViewBase<CharGenCustomPortraitCreatorVM>
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_DescriptionLabel;

	[SerializeField]
	private TextMeshProUGUI m_OpenFolderLabel;

	[SerializeField]
	private TextMeshProUGUI m_RefreshPortraitLabel;

	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[Header("Views")]
	[SerializeField]
	private CharGenPortraitView m_PortraitHalf;

	[SerializeField]
	private CharGenPortraitView m_PortraitSmall;

	[SerializeField]
	private CharGenPortraitView m_PortraitView;

	[Header("Buttons")]
	[SerializeField]
	protected OwlcatButton m_OpenFolderButton;

	[SerializeField]
	protected OwlcatButton m_RefreshPortraitButton;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_Animator.Initialize();
			m_PortraitHalf.Initialize();
			m_PortraitSmall.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Show();
		AddDisposable(base.ViewModel.Portrait.Subscribe(delegate(CharGenPortraitVM vm)
		{
			m_PortraitHalf.Bind(vm);
			m_PortraitSmall.Bind(vm);
			m_PortraitView.Or(null)?.Bind(vm);
			m_DescriptionLabel.text = UIStrings.Instance.CharGen.UploadPortraitManual;
		}));
		AddDisposable(m_OpenFolderButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnOpenFolderClick();
		}));
		AddDisposable(m_RefreshPortraitButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnRefreshPortraitClick();
		}));
		SetupLabels();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	protected virtual void Show()
	{
		m_Animator.AppearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
	}

	protected virtual void Hide()
	{
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		m_Animator.DisappearAnimation();
	}

	private void SetupLabels()
	{
		m_OpenFolderLabel.text = UIStrings.Instance.CharGen.OpenPortraitFolder;
		m_RefreshPortraitLabel.text = UIStrings.Instance.CharGen.RefreshPortrait;
		m_HeaderLabel.text = UIStrings.Instance.CharGen.CustomPortraitHeader;
	}
}
