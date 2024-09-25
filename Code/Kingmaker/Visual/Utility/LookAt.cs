using UnityEngine;

namespace Kingmaker.Visual.Utility;

[ExecuteInEditMode]
public class LookAt : MonoBehaviour
{
	public Transform lookAtTarget;

	public bool inEditor;

	private void Update()
	{
		if ((Application.isPlaying || inEditor) && (bool)lookAtTarget)
		{
			base.transform.LookAt(lookAtTarget);
		}
	}
}
