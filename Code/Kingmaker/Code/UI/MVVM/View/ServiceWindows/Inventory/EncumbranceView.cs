using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public class EncumbranceView : ViewBase<EncumbranceVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private Slider m_CurrentLoadSlider;

	[SerializeField]
	private TextMeshProUGUI m_CurrentLoadWeight;

	[SerializeField]
	private Color m_LightColor;

	[SerializeField]
	private Color m_MiddleColor;

	[SerializeField]
	private Color m_HeavyColor;

	[SerializeField]
	private Color m_OverloadColor;

	[SerializeField]
	private Graphic m_FillGraphic;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentWeightRatio.Subscribe(delegate(float value)
		{
			m_CurrentLoadSlider.value = value;
			SetGradient();
		}));
		AddDisposable(base.ViewModel.LoadWeight.Subscribe(delegate(string value)
		{
			m_CurrentLoadWeight.text = value;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetGradient()
	{
		Gradient gradient = new Gradient();
		gradient.mode = GradientMode.Blend;
		gradient.colorKeys = new GradientColorKey[4]
		{
			new GradientColorKey(m_LightColor, 0f),
			new GradientColorKey(m_MiddleColor, (float)base.ViewModel.Medium.Value / (float)base.ViewModel.Heavy.Value),
			new GradientColorKey(m_HeavyColor, 0.9f),
			new GradientColorKey(m_OverloadColor, 1f)
		};
		Gradient gradient2 = gradient;
		m_FillGraphic.color = gradient2.Evaluate(base.ViewModel.CurrentWeightRatio.Value);
	}
}
