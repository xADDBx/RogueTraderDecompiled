using Kingmaker.Blueprints.Camera;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public interface ICameraFollowTask
{
	CameraFollowTaskParamsEntry TaskParams { get; }

	TargetWrapper Owner { get; }

	Vector3 Position { get; }

	int Priority { get; }

	CameraFollowTaskState TaskState { get; }

	bool IsActive { get; }

	void Start();

	void Reset(float lifeTime);

	void SetTaskState(CameraFollowTaskState newState);
}
