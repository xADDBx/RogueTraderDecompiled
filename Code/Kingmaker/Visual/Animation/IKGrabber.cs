using DG.DemiLib.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[ScriptExecutionOrder(100)]
public class IKGrabber : MonoBehaviour
{
	public Transform GrabTargetTransform;

	public Transform GrabBone;

	private void LateUpdate()
	{
		if (!(GrabBone == null) && !(GrabTargetTransform == null))
		{
			GrabBone.position = GrabTargetTransform.position;
		}
	}
}
