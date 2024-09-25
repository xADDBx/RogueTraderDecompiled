using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

public class FxAdapter : MonoBehaviour
{
	public ParticlesSnapMap Target;

	public GameObject Prefab;

	private GameObject m_Fx;

	private void OnEnable()
	{
		if (!m_Fx)
		{
			m_Fx = FxHelper.SpawnFxOnGameObject(Prefab, Target.gameObject);
		}
	}

	private void OnDisable()
	{
		if ((bool)m_Fx)
		{
			FxHelper.Stop(m_Fx);
			m_Fx = null;
		}
	}
}
