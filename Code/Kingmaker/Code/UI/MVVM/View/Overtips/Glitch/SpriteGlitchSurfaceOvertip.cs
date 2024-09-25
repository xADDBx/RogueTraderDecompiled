using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Fx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Glitch;

public class SpriteGlitchSurfaceOvertip : MonoBehaviour
{
	[SerializeField]
	private List<Image> m_ImagesForGlitching = new List<Image>();

	public void OnEnable()
	{
		SetLowGlitch();
	}

	public void SetLowGlitch()
	{
		Material mat = FxRoot.Instance.SpriteGlitchSurfaceOvertipSettings.GetRandomLowGlitchMaterial();
		m_ImagesForGlitching.ForEach(delegate(Image i)
		{
			i.material = mat;
		});
	}

	public void SetIntensivity(float intensivity)
	{
		Material mat = FxRoot.Instance.SpriteGlitchSurfaceOvertipSettings.GetGlitchByIntensivity(intensivity);
		m_ImagesForGlitching.ForEach(delegate(Image i)
		{
			i.material = mat;
		});
	}
}
