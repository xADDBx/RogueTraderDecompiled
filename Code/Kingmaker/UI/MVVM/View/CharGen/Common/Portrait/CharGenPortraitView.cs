using System;
using JetBrains.Annotations;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Portrait;

public class CharGenPortraitView : ViewBase<CharGenPortraitVM>
{
	private enum PortraitSize
	{
		Small,
		Middle,
		Full
	}

	[SerializeField]
	private CharBPortraitChanger m_Portrait;

	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private PortraitSize m_Size = PortraitSize.Full;

	[SerializeField]
	private OwlcatButton m_HoverButton;

	[UsedImplicitly]
	private bool m_IsInit;

	[UsedImplicitly]
	private bool m_IsShown;

	private Action<bool> m_HoverAction;

	private Sprite PortraitSmall => base.ViewModel.PortraitSmall;

	private Sprite PortraitHalf => base.ViewModel.PortraitHalf;

	private Sprite PortraitFull => base.ViewModel.PortraitFull;

	public void Initialize(Action<bool> hoverAction = null)
	{
		if (!m_IsInit)
		{
			m_HoverAction = hoverAction;
			m_Animator.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Initialize();
		if (CheckPortrait())
		{
			Show();
			SetupView();
		}
		else
		{
			Hide();
		}
		if (m_HoverButton != null)
		{
			AddDisposable(m_HoverButton.OnHoverAsObservable().Subscribe(delegate(bool value)
			{
				m_HoverAction?.Invoke(value);
			}));
		}
	}

	private bool CheckPortrait()
	{
		return m_Size switch
		{
			PortraitSize.Small => PortraitSmall != null, 
			PortraitSize.Middle => PortraitHalf != null, 
			PortraitSize.Full => PortraitFull != null, 
			_ => false, 
		};
	}

	private void SetupView()
	{
		switch (m_Size)
		{
		case PortraitSize.Small:
			m_Portrait.SetNewPortrait(PortraitSmall, playAnimation: true, playSound: true);
			break;
		case PortraitSize.Middle:
			m_Portrait.SetNewPortrait(PortraitHalf, playAnimation: true, playSound: true);
			break;
		case PortraitSize.Full:
			m_Portrait.SetNewPortrait(PortraitFull, playAnimation: true, playSound: true);
			break;
		}
	}

	public void SetVisibility(bool visible)
	{
		if (visible)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Show()
	{
		if (!m_IsShown)
		{
			if (m_Animator != null)
			{
				m_Animator.AppearAnimation();
			}
			else
			{
				base.gameObject.SetActive(value: true);
			}
			m_IsShown = true;
		}
	}

	public void Hide()
	{
		if (m_Animator != null)
		{
			m_Animator.DisappearAnimation();
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		m_IsShown = false;
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}
}
