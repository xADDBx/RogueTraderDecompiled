using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipVoidshipHealthView : ViewBase<OvertipVoidshipHealthVM>
{
	[SerializeField]
	private Image m_VoidshipHealthBar;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		AddDisposable(base.ViewModel.MaxShipHealth.Subscribe(delegate
		{
			SetVoidshipHealth();
		}));
		AddDisposable(base.ViewModel.CurrentShipHealth.Subscribe(delegate
		{
			SetVoidshipHealth();
		}));
	}

	private void SetVoidshipHealth()
	{
		if (base.ViewModel.CurrentShipHealth.Value > 0 && base.ViewModel.MaxShipHealth.Value > 0 && base.ViewModel.CurrentShipHealth.Value != base.ViewModel.MaxShipHealth.Value)
		{
			m_FadeAnimator.AppearAnimation();
			m_VoidshipHealthBar.DOFillAmount((float)base.ViewModel.CurrentShipHealth.Value / (float)base.ViewModel.MaxShipHealth.Value, 0.5f);
		}
		else if (base.ViewModel.CurrentShipHealth.Value > 0 && base.ViewModel.MaxShipHealth.Value > 0 && base.ViewModel.CurrentShipHealth.Value == base.ViewModel.MaxShipHealth.Value)
		{
			m_VoidshipHealthBar.DOFillAmount(1f, 0.5f);
			DelayedInvoker.InvokeInTime(delegate
			{
				m_FadeAnimator.DisappearAnimation();
			}, 0.5f);
		}
		else if (base.ViewModel.CurrentShipHealth.Value == 0)
		{
			m_VoidshipHealthBar.fillAmount = 0f;
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
