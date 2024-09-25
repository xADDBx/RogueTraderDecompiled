using Kingmaker.Blueprints.Camera;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public abstract class BaseCameraFollowTask : ICameraFollowTask
{
	public CameraTaskType Type;

	public CameraFollowTaskParamsEntry TaskParams { get; }

	public TargetWrapper Owner { get; protected set; }

	public Vector3 Position { get; protected set; } = Vector3.zero;


	public int Priority { get; protected set; }

	public CameraFollowTaskState TaskState { get; protected set; }

	public virtual bool IsActive { get; protected set; }

	protected BaseCameraFollowTask(CameraFollowTaskParamsEntry taskParams, TargetWrapper owner, int priority = 0)
	{
		TaskParams = taskParams;
		Owner = owner;
		Priority = priority;
	}

	public abstract void Start();

	public abstract void Reset(float lifeTime);

	public void SetTaskState(CameraFollowTaskState newState)
	{
		TaskState = newState;
	}
}
