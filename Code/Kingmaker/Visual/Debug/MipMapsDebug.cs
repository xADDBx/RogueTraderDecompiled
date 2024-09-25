using UnityEngine;

namespace Kingmaker.Visual.Debug;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class MipMapsDebug : MonoBehaviour
{
	private Camera m_Camera;

	private Shader m_Shader;

	private Texture2D m_DebugTexture;

	public bool IsEnabled;

	public Shader Shader
	{
		get
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("Hidden/MipMapsDebug");
			}
			return m_Shader;
		}
	}
}
