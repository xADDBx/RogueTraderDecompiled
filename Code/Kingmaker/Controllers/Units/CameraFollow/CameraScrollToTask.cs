using System;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class CameraScrollToTask : BaseCameraFollowTask
{
	private TimeSpan m_LifeTime;

	private TimeSpan m_FinishTime;

	public override bool IsActive => Game.Instance.TimeController.RealTime < m_FinishTime;

	public CameraScrollToTask(CameraFollowTaskParamsEntry taskParams, TargetWrapper owner, Vector3? position, int priority)
		: base(taskParams, owner, priority)
	{
		base.Position = position ?? owner.Point;
		m_LifeTime = (taskParams.CameraFlyParams.MaxTime + taskParams.CameraObserveTime).Seconds();
	}

	public override void Start()
	{
		m_FinishTime = Game.Instance.TimeController.RealTime + m_LifeTime;
	}

	public override void Reset(float lifeTime)
	{
		m_LifeTime = lifeTime.Seconds();
		m_FinishTime = Game.Instance.TimeController.RealTime + m_LifeTime;
	}
}
