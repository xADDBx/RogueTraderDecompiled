using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class WallFXRotator : MonoBehaviour
{
	private void Start()
	{
		CameraRig cameraRig = Object.FindObjectOfType<CameraRig>();
		if (null != cameraRig)
		{
			base.transform.rotation = Quaternion.Euler(new Vector3(base.transform.rotation.x, cameraRig.transform.rotation.eulerAngles.y - 45f, base.transform.rotation.z));
			if (Mathf.Abs(cameraRig.transform.rotation.eulerAngles.y - 135f) < 1f || Mathf.Abs(cameraRig.transform.rotation.eulerAngles.y - 315f) < 1f)
			{
				base.transform.localScale = new Vector3(base.transform.localScale.z, base.transform.localScale.y, base.transform.localScale.x);
			}
		}
	}
}
