using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.View.Spawners.Projectile;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers.Projectiles;

public class ProjectileSpawnerController : IControllerTick, IController
{
	private readonly HashSet<ProjectileSpawner> m_ProjectileSpawners = new HashSet<ProjectileSpawner>();

	public void RegisterSpawner(ProjectileSpawner spawner)
	{
		m_ProjectileSpawners.Add(spawner);
	}

	public void UnregisterSpawner(ProjectileSpawner spawner)
	{
		m_ProjectileSpawners.Remove(spawner);
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (m_ProjectileSpawners.Count == 0)
		{
			return;
		}
		List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (IsApplicableUnit(partyAndPet))
			{
				list.Add(partyAndPet);
			}
		}
		bool flag = Game.Instance.IsModeActive(GameModeType.Dialog);
		bool flag2 = Game.Instance.IsModeActive(GameModeType.Cutscene);
		float currentTime = (float)Game.Instance.TimeController.RealTime.TotalSeconds;
		float deltaTime = Game.Instance.TimeController.DeltaTime;
		foreach (ProjectileSpawner projectileSpawner in m_ProjectileSpawners)
		{
			if ((flag2 && !projectileSpawner.ActiveInCutscenes) || (flag && !projectileSpawner.ActiveInDialogues))
			{
				projectileSpawner.Pause();
			}
			else
			{
				projectileSpawner.Resume();
			}
			projectileSpawner.Tick(list, currentTime, deltaTime);
		}
	}

	private static bool IsApplicableUnit(BaseUnitEntity unit)
	{
		if (unit.IsInGame && !unit.Stealth.Active)
		{
			return unit.LifeState.State == UnitLifeState.Conscious;
		}
		return false;
	}
}
