using UnityEngine;

namespace Kingmaker.Visual.Debug;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class StaticSceneDebug : MonoBehaviour
{
	private Shader m_Shader;

	private Camera m_Camera;

	private MaterialPropertyBlock m_PropertyBlock;

	public bool IsEnabled;

	public Color ColorStatic = new Color(0.2f, 1f, 0.2f, 1f);

	public Color ColorDynamic = new Color(1f, 0.2f, 0.2f, 1f);

	public Shader Shader
	{
		get
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("Hidden/StaticSceneDebug");
			}
			return m_Shader;
		}
	}
}
