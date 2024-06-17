using Kingmaker.UI.MVVM.VM.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMap;

public class SystemMapNoisesView : ViewBase<SystemMapNoisesVM>
{
	[SerializeField]
	private GameObject m_AnomalyNoise;

	[SerializeField]
	private GameObject m_PlanetPoiNoise;

	[SerializeField]
	private GameObject m_PlanetResourceNoise;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.AnomalyIsNear.Subscribe(m_AnomalyNoise.SetActive));
		AddDisposable(base.ViewModel.PoiIsNear.Subscribe(m_PlanetPoiNoise.SetActive));
		AddDisposable(base.ViewModel.ResourcesIsNear.Subscribe(m_PlanetResourceNoise.SetActive));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
