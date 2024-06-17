using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil;

public abstract class SurfaceMomentumView : ViewBase<SurfaceMomentumVM>
{
	[FormerlySerializedAs("SurfaceMomentumEntityPCView")]
	[SerializeField]
	protected SurfaceMomentumEntityView m_SurfaceMomentumEntityView;

	[SerializeField]
	protected WidgetListMVVM m_HeroicActWidgetList;

	[SerializeField]
	protected WidgetListMVVM m_DesperateMeasureWidgetList;

	[FormerlySerializedAs("m_SlotPCView")]
	[SerializeField]
	private SurfaceActionBarSlotAbilityView m_SlotView;

	[SerializeField]
	private FadeAnimator m_Animator;

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	public void Initialize()
	{
		m_Animator.Initialize();
		m_SurfaceMomentumEntityView.Initialize();
		m_Animator.DisappearAnimation();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsTurnBasedActive.CombineLatest(base.ViewModel.IsPlayerTurn, base.ViewModel.IsAppropriateGameMode, (bool isTurnBasedActive, bool isPlayerTurn, bool isAppropriateGameMode) => new { isTurnBasedActive, isPlayerTurn, isAppropriateGameMode }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			m_Animator.PlayAnimation(value.isAppropriateGameMode && value.isTurnBasedActive && value.isPlayerTurn);
		}));
		AddDisposable(base.ViewModel.MomentumEntityVM.Subscribe(m_SurfaceMomentumEntityView.Bind));
		AddDisposable(base.ViewModel.UnitChanged.Subscribe(DrawEntries));
		AddDisposable(base.ViewModel.AbilitiesListUpdated.ObserveLastValueOnLateUpdate().Subscribe(DrawEntries));
		DrawEntries();
	}

	protected override void DestroyViewImplementation()
	{
		m_Disposable?.Clear();
		m_Disposable?.Dispose();
		m_Disposable = null;
		m_Animator.DisappearAnimation();
	}

	private void DrawEntries()
	{
		m_Disposable?.Clear();
		if (!(base.ViewModel.Unit == null))
		{
			m_Disposable?.Add(m_HeroicActWidgetList.DrawEntries(base.ViewModel.HeroicActSlots, m_SlotView));
			m_Disposable?.Add(m_DesperateMeasureWidgetList.DrawEntries(base.ViewModel.DesperateMeasureSlots, m_SlotView));
		}
	}
}
