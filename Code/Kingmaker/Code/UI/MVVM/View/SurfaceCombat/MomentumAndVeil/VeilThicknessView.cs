using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil;

public abstract class VeilThicknessView : ViewBase<VeilThicknessVM>
{
	[SerializeField]
	protected Image m_TooltipArea;

	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private DelayedSlider m_ValueSlider;

	[SerializeField]
	private Slider m_PredictedValueSlider;

	[SerializeField]
	private Slider m_CriticalValueSlider;

	[SerializeField]
	private OwlcatMultiSelectable[] m_Effects;

	private int m_TextValue;

	public void Initialize()
	{
		m_ValueSlider.Initialize();
		m_Animator.Initialize();
		m_Animator.DisappearAnimation();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		int maximumVeilOnAllLocation = BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot.MaximumVeilOnAllLocation;
		int criticalVeil = BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot.CriticalVeilOnAllLocation;
		m_ValueSlider.SetMaxValue(maximumVeilOnAllLocation);
		m_PredictedValueSlider.maxValue = maximumVeilOnAllLocation;
		m_CriticalValueSlider.maxValue = maximumVeilOnAllLocation;
		m_CriticalValueSlider.value = criticalVeil;
		AddDisposable(base.ViewModel.IsTurnBasedActive.CombineLatest(base.ViewModel.IsPlayerTurn, base.ViewModel.IsAppropriateGameMode, (bool isTurnBasedActive, bool isPlayerTurn, bool isAppropriateGameMode) => new { isTurnBasedActive, isPlayerTurn, isAppropriateGameMode }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			m_Animator.PlayAnimation(value.isAppropriateGameMode && value.isTurnBasedActive && value.isPlayerTurn);
		}));
		AddDisposable(base.ViewModel.Value.Subscribe(delegate(int value)
		{
			m_ValueSlider.SetValue(value);
			OwlcatMultiSelectable[] effects = m_Effects;
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].SetActiveLayer((value < criticalVeil) ? 1 : 0);
			}
		}));
		AddDisposable(base.ViewModel.PredictedValue.Subscribe(delegate(int value)
		{
			m_PredictedValueSlider.value = value;
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_Animator.DisappearAnimation();
	}
}
