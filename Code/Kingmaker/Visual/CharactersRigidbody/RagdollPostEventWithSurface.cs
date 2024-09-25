using Kingmaker.View;
using Kingmaker.Visual.Animation;
using UnityEngine;

namespace Kingmaker.Visual.CharactersRigidbody;

public class RagdollPostEventWithSurface : MonoBehaviour
{
	[SerializeField]
	private UnitEntityView m_View;

	[SerializeField]
	private UnitAnimationCallbackReceiver m_Receiver;

	[SerializeField]
	private RigidbodyCreatureController m_Controller;

	public string SoundString = "BodyfallsRagDoll_Play";

	public void GetInfoAboutCharacter()
	{
		m_View = GetComponentInParent<UnitEntityView>();
		if (m_View == null)
		{
			return;
		}
		m_Controller = m_View.GetComponentInChildren<RigidbodyCreatureController>();
		if (!(m_Controller == null))
		{
			m_Receiver = m_View.GetComponentInChildren<UnitAnimationCallbackReceiver>();
			if (m_Receiver == null)
			{
				m_Receiver = m_Controller.gameObject.AddComponent<UnitAnimationCallbackReceiver>();
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer != 29 && !(m_View == null) && !(m_Receiver == null) && !(m_Controller == null) && m_Controller.PostEventWithSurface)
		{
			m_Receiver.PlayBodyFall(SoundString);
			m_Controller.PostEventWithSurface = false;
		}
	}
}
