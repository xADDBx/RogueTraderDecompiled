using Kingmaker.View;
using UnityEngine;

public class DecalRotator : MonoBehaviour
{
	private void Start()
	{
		CameraRig cameraRig = Object.FindObjectOfType<CameraRig>();
		if (null != cameraRig)
		{
			base.transform.rotation = Quaternion.Euler(new Vector3(base.transform.rotation.x, cameraRig.transform.rotation.eulerAngles.y, base.transform.rotation.z));
		}
	}
}
