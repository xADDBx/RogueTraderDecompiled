using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil;

public abstract class SurfaceMomentumEntityView : ViewBase<MomentumEntityVM>
{
	[SerializeField]
	private DelayedSlider m_CurrentSlider;

	[SerializeField]
	private Slider m_HeroicActSlider;

	[SerializeField]
	private Slider m_DesperateMeasureSlider;

	[SerializeField]
	private OwlcatMultiSelectable m_HeroicActSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_DesperateMeasureSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_LabelSelectable;

	protected static int DefaultMomentumValue => Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot.StartingMomentum;

	public void Initialize()
	{
		m_CurrentSlider.Initialize();
		ResetView();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentPercent.Subscribe(delegate(float value)
		{
			m_CurrentSlider.SetValue(value);
		}));
		AddDisposable(base.ViewModel.HeroicActPercent.Subscribe(delegate(float value)
		{
			m_HeroicActSlider.value = value;
		}));
		AddDisposable(base.ViewModel.DesperateMeasurePercent.Subscribe(delegate(float value)
		{
			m_DesperateMeasureSlider.value = value;
		}));
		AddDisposable(base.ViewModel.HeroicActActive.Subscribe(delegate(bool value)
		{
			m_HeroicActSelectable.SetActiveLayer(value ? "Active" : "NotActive");
			m_LabelSelectable.SetActiveLayer(value ? "Normal" : "Disable");
			if (value)
			{
				UISounds.Instance.Sounds.Combat.MomentumHighlightOn.Play();
			}
			else
			{
				UISounds.Instance.Sounds.Combat.MomentumHighlightOff.Play();
			}
		}));
		AddDisposable(base.ViewModel.DesperateMeasureActive.Subscribe(delegate(bool value)
		{
			m_DesperateMeasureSelectable.SetActiveLayer(value ? "Active" : "NotActive");
			if (value)
			{
				UISounds.Instance.Sounds.Combat.MomentumHighlightOn.Play();
			}
			else
			{
				UISounds.Instance.Sounds.Combat.MomentumHighlightOff.Play();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		ResetView();
	}

	protected virtual void ResetView()
	{
		m_CurrentSlider.SetValue(0.5f, showDelta: false);
		m_HeroicActSlider.value = 0.75f;
		m_DesperateMeasureSlider.value = 0.25f;
		m_HeroicActSelectable.SetActiveLayer("NotActive");
		m_LabelSelectable.SetActiveLayer("Disable");
		m_DesperateMeasureSelectable.SetActiveLayer("NotActive");
	}

	public void HighlightHeroicAct(bool value)
	{
		if (m_HeroicActSelectable.IsHighlighted != value)
		{
			if (value)
			{
				m_HeroicActSelectable.OnPointerEnter();
			}
			else
			{
				m_HeroicActSelectable.OnPointerExit();
			}
		}
	}

	public void HighlightDesperateMeasure(bool value)
	{
		if (m_DesperateMeasureSelectable.IsHighlighted != value)
		{
			if (value)
			{
				m_DesperateMeasureSelectable.OnPointerEnter();
			}
			else
			{
				m_DesperateMeasureSelectable.OnPointerExit();
			}
		}
	}
}
