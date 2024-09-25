using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public abstract class CharInfoComponentView<TViewModel> : ViewBase<TViewModel>, ICharInfoComponentView where TViewModel : CharInfoComponentVM
{
	[SerializeField]
	protected FadeAnimator m_FadeAnimator;

	public virtual void Initialize()
	{
		m_FadeAnimator.Or(null)?.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		Show();
		AddDisposable(base.ViewModel.Unit?.Subscribe(delegate
		{
			RefreshView();
		}));
		if (base.ViewModel.Unit == null)
		{
			RefreshView();
		}
	}

	protected virtual void RefreshView()
	{
	}

	public void BindSection(CharInfoComponentVM vm)
	{
		Bind(vm as TViewModel);
	}

	public void UnbindSection()
	{
		Unbind();
	}

	private void Show()
	{
		OnShow();
		base.gameObject.SetActive(value: true);
		if ((bool)m_FadeAnimator)
		{
			m_FadeAnimator.AppearAnimation();
		}
	}

	public void ShowLeftCanvasSound()
	{
		UISounds.Instance.Sounds.Character.CharacterStatsShow.Play();
	}

	public void ShowRightCanvasSound()
	{
		UISounds.Instance.Sounds.Character.CharacterInfoShow.Play();
	}

	protected virtual void OnShow()
	{
	}

	public void Hide()
	{
		OnHide();
		base.gameObject.SetActive(value: false);
		if ((bool)m_FadeAnimator)
		{
			m_FadeAnimator.DisappearAnimation();
		}
	}

	protected virtual void OnHide()
	{
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
