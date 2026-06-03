using Kingmaker.Code.UI.MVVM.VM.BossHPBar;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.BossHPBar;

public class BossHPBarCommonView : ViewBase<BossHPBarVM>
{
	[SerializeField]
	private GameObject m_ContentObject;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_HPLabel;

	[SerializeField]
	private Image m_SliderImage;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		m_ContentObject.transform.localPosition = (Game.Instance.IsControllerGamepad ? new Vector3(0f, 350f, 0f) : new Vector3(0f, 370f, 0f));
		AddDisposable(base.ViewModel.BossName.Subscribe(delegate(string value)
		{
			m_Label.text = value;
		}));
		AddDisposable(base.ViewModel.HPLabel.Subscribe(delegate(string value)
		{
			m_HPLabel.text = value;
		}));
		AddDisposable(base.ViewModel.Progress.Subscribe(delegate(float value)
		{
			m_SliderImage.fillAmount = value;
		}));
		AddDisposable(base.ViewModel.IsShowing.Subscribe(OnShowHide));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void OnShowHide(bool showing)
	{
		if (showing)
		{
			base.gameObject.SetActive(value: true);
			m_FadeAnimator.AppearAnimation();
		}
		else
		{
			m_FadeAnimator.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
			});
		}
	}
}
