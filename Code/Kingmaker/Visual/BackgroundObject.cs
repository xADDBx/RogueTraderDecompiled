using UnityEngine;

namespace Kingmaker.Visual;

public class BackgroundObject : MonoBehaviour
{
	private RenderingManager m_RenderingManager;

	private void Update()
	{
		if ((bool)Game.GetCamera() && !m_RenderingManager)
		{
			m_RenderingManager = Game.GetCamera().GetComponent<RenderingManager>();
		}
		if ((bool)m_RenderingManager)
		{
			m_RenderingManager.Background = base.gameObject;
		}
	}
}
