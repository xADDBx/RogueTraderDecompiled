using Kingmaker.Code.UI.MVVM.VM.IngameMenu;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.IngameMenu.PC;

public abstract class IngameMenuBasePCView<TViewModel> : ViewBase<TViewModel> where TViewModel : IngameMenuBaseVM
{
	[SerializeField]
	private FadeAnimator m_Animator;

	public virtual void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(SwitchVisibility));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SwitchVisibility(bool state)
	{
		if (state)
		{
			m_Animator.AppearAnimation();
			return;
		}
		m_Animator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}
}
