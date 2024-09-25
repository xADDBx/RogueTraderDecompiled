using UnityEngine;

namespace Kingmaker.Visual.CharactersRigidbody;

public class RememberRotationForTorchIkKastil : MonoBehaviour
{
	public Vector3 Rotation;

	private void Start()
	{
		Rotation = base.transform.localRotation.eulerAngles;
	}
}
