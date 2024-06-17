using Kingmaker.Blueprints.Camera;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public sealed class CameraFollowTask : BaseCameraFollowTask
{
	public CameraFollowTask(CameraFollowTaskParamsEntry taskParams, TargetWrapper owner, Vector3? position)
		: base(taskParams, owner)
	{
		base.Position = position ?? owner.Point;
		base.TaskState = CameraFollowTaskState.Observe;
		IsActive = true;
	}

	public override void Start()
	{
		IsActive = false;
	}

	public override void Reset(float lifeTime)
	{
	}
}
