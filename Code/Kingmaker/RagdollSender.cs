using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker;

public class RagdollSender : MonoBehaviour
{
	public RagdollRecieverMain Receiver;

	private void OnCollisionEnter(Collision collision)
	{
		if ((bool)Receiver)
		{
			Receiver.Send(base.gameObject.name, collision.relativeVelocity.magnitude, collision.gameObject.GetComponent<SoundSurfaceObject>()?.Switch);
		}
	}
}
