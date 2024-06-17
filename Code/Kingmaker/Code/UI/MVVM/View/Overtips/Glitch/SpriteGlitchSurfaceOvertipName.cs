using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Fx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Glitch;

public class SpriteGlitchSurfaceOvertipName : MonoBehaviour
{
	[SerializeField]
	private List<Image> m_ImagesForGlitching = new List<Image>();

	public void OnEnable()
	{
		Material mat = FxRoot.Instance.SpriteGlitchSurfaceOvertipSettings.GetRandomMaterial();
		m_ImagesForGlitching.ForEach(delegate(Image i)
		{
			i.material = mat;
		});
	}
}
