using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.DarkHeresyPopUp;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DarkHeresyPopUp;

public class DarkHeresyPopUpView : ViewBase<DarkHeresyPopUpVM>
{
	[SerializeField]
	private FadeAnimator m_MainContainer;

	[SerializeField]
	protected OwlcatMultiButton m_WishlistButton;

	[SerializeField]
	private TextMeshProUGUI m_WishlistButtonLabel;

	[SerializeField]
	private TextMeshProUGUI m_PopUpLabel;

	[SerializeField]
	private TextMeshProUGUI m_PopUpSubLabel;

	private bool m_IsShowed;

	public void Initialize()
	{
		m_MainContainer.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		Show();
		m_PopUpLabel.text = UIStrings.Instance.UIDarkHeresyPopUp.Label;
		m_PopUpSubLabel.text = UIStrings.Instance.UIDarkHeresyPopUp.SubLabel;
		m_WishlistButtonLabel.text = UIStrings.Instance.UIDarkHeresyPopUp.WishlistButtonLabel;
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	public virtual void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			base.gameObject.SetActive(value: true);
			m_MainContainer.AppearAnimation();
			AkSoundEngine.SetState("MusicState", "DH_PopUp");
			UISounds.Instance.Sounds.DarkHeresyPopUp.PopUpShow.Play();
			SetStoreIconVisibility(visible: true);
		}
	}

	public virtual void Hide()
	{
		if (m_IsShowed)
		{
			SoundState.Instance?.MusicStateHandler.SetMusicState(MusicStateHandler.MusicState.MainMenu);
			UISounds.Instance.Sounds.DarkHeresyPopUp.PopUpHide.Play();
			SetStoreIconVisibility(visible: false);
			m_MainContainer.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				m_IsShowed = false;
			});
		}
	}

	private void SetStoreIconVisibility(bool visible)
	{
	}
}
