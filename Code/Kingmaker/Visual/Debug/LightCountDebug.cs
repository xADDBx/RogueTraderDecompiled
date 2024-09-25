using UnityEngine;

namespace Kingmaker.Visual.Debug;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class LightCountDebug : MonoBehaviour
{
	private Camera m_Camera;

	private Shader m_Shader;

	public bool IsEnabled;

	public Shader Shader
	{
		get
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("Hidden/LightsCountDebug");
			}
			return m_Shader;
		}
	}
}
