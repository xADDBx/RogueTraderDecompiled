using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Debugging;

public class MipMapDebug
{
	private readonly Color[] m_DebugColors = new Color[6]
	{
		new Color(0f, 0f, 1f, 0.8f),
		new Color(0f, 0.5f, 1f, 0.4f),
		new Color(1f, 1f, 1f, 0f),
		new Color(1f, 0.7f, 0f, 0.2f),
		new Color(1f, 0.3f, 0f, 0.6f),
		new Color(1f, 0f, 0f, 0.8f)
	};

	private Texture2D m_DebugTexture;

	public void Prepare(CommandBuffer cmd)
	{
		if (m_DebugTexture == null)
		{
			int num = 32;
			int num2 = 0;
			m_DebugTexture = new Texture2D(num, num, TextureFormat.RGBA32, mipChain: true);
			m_DebugTexture.name = "_MipMapDebugMap";
			while (num >= 1)
			{
				Color[] array = new Color[num * num];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = m_DebugColors[num2];
				}
				m_DebugTexture.SetPixels(array, num2);
				num2++;
				num /= 2;
			}
			m_DebugTexture.filterMode = FilterMode.Trilinear;
			m_DebugTexture.Apply(updateMipmaps: false);
		}
		cmd.SetGlobalTexture(DebugBuffer._MipMapDebugMap, m_DebugTexture);
	}
}
