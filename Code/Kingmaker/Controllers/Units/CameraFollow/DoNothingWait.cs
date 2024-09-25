using System;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class DoNothingWait : BaseCameraFollowTask
{
	private TimeSpan m_LifeTime;

	private TimeSpan m_FinishTime;

	public override bool IsActive => Game.Instance.TimeController.RealTime < m_FinishTime;

	public DoNothingWait(CameraFollowTaskParamsEntry taskParams, TargetWrapper owner, int priority)
		: base(taskParams, owner, priority)
	{
		m_LifeTime = ((base.TaskParams != null) ? (base.TaskParams.CameraFlyParams.MaxTime + base.TaskParams.CameraObserveTime) : 2f).Seconds();
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
