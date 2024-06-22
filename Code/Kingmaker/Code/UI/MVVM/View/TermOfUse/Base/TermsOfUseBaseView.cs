using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.Code.UI.MVVM.VM.TermOfUse;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.TermOfUse.Base;

public class TermsOfUseBaseView : ViewBase<TermsOfUseVM>
{
	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_MainContainer;

	[SerializeField]
	[UsedImplicitly]
	private MoveAnimator m_Header;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Licence;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_SubLicence;

	private bool m_IsShowed;

	protected static bool IsShowFirstTime => !FirstLaunchSettingsVM.HasShown;

	public void Initialize()
	{
		PFLog.System.Log("Initializing terms of use window...");
		m_MainContainer.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_Title.text = base.ViewModel.TermsOfUseTexts.Header;
		m_Licence.text = base.ViewModel.GetLicenceText();
		m_SubLicence.text = base.ViewModel.TermsOfUseTexts.SubLicence;
		Show();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			m_MainContainer.AppearAnimation();
			m_Header.AppearAnimation();
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Settings);
			});
			UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
		}
	}

	public void Hide()
	{
		if (m_IsShowed)
		{
			m_Header.DisappearAnimation();
			m_MainContainer.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				m_IsShowed = false;
			});
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Settings);
			});
			UISounds.Instance.Sounds.LocalMap.MapClose.Play();
		}
	}
}
