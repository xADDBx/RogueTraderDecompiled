using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Bark.Base;

public class StarSystemSpaceBarksHolderBaseView<TStarSystemSpaceBarkView> : ViewBase<StarSystemSpaceBarksHolderVM> where TStarSystemSpaceBarkView : StarSystemSpaceBarkBaseView
{
	[SerializeField]
	private TStarSystemSpaceBarkView m_StarSystemSpaceBarkView;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private float m_HideDelay = 3f;

	[SerializeField]
	private float m_FadeDuration = 0.5f;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.BarksVMs.ObserveAdd().Subscribe(delegate(CollectionAddEvent<StarSystemSpaceBarkVM> value)
		{
			AddBarkView(value.Value);
		}));
		AddDisposable(base.ViewModel.IsVisible.Subscribe(SetVisibility));
		AddDisposable(base.ViewModel.BlockHideRequested.Subscribe(delegate
		{
			StartBlockHide();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void AddInput(InputLayer inputLayer)
	{
		m_StarSystemSpaceBarkView.AddInput(inputLayer);
	}

	private void AddBarkView(StarSystemSpaceBarkVM vm)
	{
		m_StarSystemSpaceBarkView.Unbind();
		m_StarSystemSpaceBarkView.Initialize();
		m_StarSystemSpaceBarkView.Bind(vm);
	}

	private void SetVisibility(bool value)
	{
		m_CanvasGroup.DOKill();
		m_CanvasGroup.alpha = (value ? 1f : 0f);
	}

	private void StartBlockHide()
	{
		DOVirtual.DelayedCall(m_HideDelay, delegate
		{
			m_CanvasGroup.DOFade(0f, m_FadeDuration).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		}).SetUpdate(isIndependentUpdate: true);
	}
}
