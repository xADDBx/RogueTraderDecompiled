using System;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;

public class EncyclopediaFullScreenPageImageBaseView : ViewBase<EncyclopediaPageImageVM>
{
	[SerializeField]
	private Image m_Image;

	[SerializeField]
	protected OwlcatButton m_CloseButton;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	private Action m_CloseAction;

	public void Initialize(Action closeAction)
	{
		m_CloseAction = closeAction;
		m_FadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_Image.sprite = base.ViewModel.Image;
		m_FadeAnimator.AppearAnimation();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void Close()
	{
		m_FadeAnimator.DisappearAnimation(delegate
		{
			m_CloseAction?.Invoke();
		});
	}
}
