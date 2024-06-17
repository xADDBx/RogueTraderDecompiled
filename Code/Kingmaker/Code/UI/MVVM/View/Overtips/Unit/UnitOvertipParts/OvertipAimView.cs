using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipAimView : ViewBase<OvertipHitChanceBlockVM>
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void BindViewImplementation()
	{
		m_CanvasGroup.alpha = 0f;
		AddDisposable(base.ViewModel.IsVisibleTrigger.CombineLatest(base.ViewModel.HasHit, base.ViewModel.HitAlways, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.HoverSelfTargetAbility, (bool isVisible, bool hasHit, bool hitAlways, bool hover, bool selfAbility) => (isVisible && (hasHit || hitAlways) && hover) || selfAbility).ObserveLastValueOnLateUpdate().Subscribe(delegate(bool b)
		{
			m_FadeAnimator.PlayAnimation(b);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
