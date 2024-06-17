using System;
using UnityEngine;

namespace Kingmaker.Visual;

[Serializable]
public class Ramp
{
	private Texture2D m_RampTexture;

	public bool Enabled;

	public Gradient Gradient = new Gradient();

	public Action<Texture2D> TextureBaked;

	public void Bake()
	{
		BakeInternal();
		TextureBaked?.Invoke(m_RampTexture);
	}

	public Texture2D GetRamp()
	{
		if (m_RampTexture == null)
		{
			BakeInternal();
		}
		return m_RampTexture;
	}

	private void BakeInternal()
	{
		if (m_RampTexture != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(m_RampTexture);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(m_RampTexture);
			}
		}
		m_RampTexture = new Texture2D(256, 1);
		Color[] array = new Color[256];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Gradient.Evaluate((float)i / 255f);
		}
		m_RampTexture.SetPixels(array);
		m_RampTexture.Apply();
	}
}
