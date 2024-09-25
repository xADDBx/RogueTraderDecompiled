using System;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Controllers.Units.CameraFollow;

public class ActionCameraTask : BaseCameraFollowTask
{
	private TimeSpan m_LifeTime;

	private TimeSpan m_FinishTime;

	public AbstractUnitEntity Caster;

	public AbstractUnitEntity Target;

	public override bool IsActive => Game.Instance.TimeController.RealTime < m_FinishTime;

	public ActionCameraTask(AbstractUnitEntity caster, AbstractUnitEntity target, int priority)
		: base(null, null, priority)
	{
		Caster = caster;
		Target = target;
		m_LifeTime = CalcFiringDurationTime(Caster, Target);
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

	private TimeSpan CalcFiringDurationTime(AbstractUnitEntity caster, AbstractUnitEntity target)
	{
		float num = Vector3.Distance(caster.Position, target.Position);
		float num2 = (caster.Commands.Current as UnitUseAbility)?.Ability.StarshipWeapon?.Ammo.Blueprint.ShotProjectile.Speed ?? 20f;
		return (num / num2 + 2f).Seconds();
	}
}
