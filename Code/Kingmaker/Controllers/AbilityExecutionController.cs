using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers;

public class AbilityExecutionController : IControllerTick, IController, IControllerStop, IControllerStart
{
	private readonly List<AbilityExecutionProcess> m_Abilities = new List<AbilityExecutionProcess>();

	private bool m_Enabled;

	public ReadonlyList<AbilityExecutionProcess> Abilities => m_Abilities;

	public AbilityExecutionProcess Execute(AbilityExecutionContext context)
	{
		if (!m_Enabled)
		{
			throw new Exception("Attempting to access AbilityExecutionController that is disabled");
		}
		AbilityExecutionProcess abilityExecutionProcess = new AbilityExecutionProcess(context);
		MechanicEntity caster = context.Caster;
		if (caster is SimpleCaster)
		{
			AbilityExecutionProcess abilityExecutionProcess2 = m_Abilities.FirstItem((AbilityExecutionProcess i) => i.Context.Caster == caster);
			if (abilityExecutionProcess2 != null)
			{
				PFLog.Default.ErrorWithReport($"{caster} already execute ability {abilityExecutionProcess2.Context.AbilityBlueprint} " + "and can't execute another ability");
				return abilityExecutionProcess;
			}
		}
		context.CastTime = Game.Instance.TimeController.GameTime;
		m_Abilities.Add(abilityExecutionProcess);
		if (caster.IsInCombat)
		{
			Game.Instance.PlayerInputInCombatController.RequestLockPlayerInput();
		}
		return abilityExecutionProcess;
	}

	public void Detach(AbilityExecutionProcess process)
	{
		if (!m_Enabled)
		{
			throw new Exception("Attempting to access AbilityExecutionController that is disabled");
		}
		bool num = process.Context?.Caster.IsInCombat ?? true;
		process.Dispose();
		if (num)
		{
			Game.Instance.PlayerInputInCombatController.RequestUnlockPlayerInput();
		}
		m_Abilities.Remove(process);
	}

	public void DetachAll()
	{
		foreach (AbilityExecutionProcess ability in m_Abilities)
		{
			bool isInCombat = ability.Context.Caster.IsInCombat;
			PFLog.Ability.Log("[AbilityExecutionController] '" + ability.Context.AbilityBlueprint.Name + "' will be detached...");
			ability.Dispose();
			if (isInCombat)
			{
				Game.Instance.PlayerInputInCombatController.RequestUnlockPlayerInput();
			}
		}
		m_Abilities.Clear();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerStart.OnStart()
	{
		m_Enabled = true;
		if (0 < m_Abilities.Count)
		{
			m_Abilities.Clear();
			throw new Exception("AbilityExecutionController.m_Abilities was changed, when controller was disabled!");
		}
	}

	void IControllerTick.Tick()
	{
		for (int i = 0; i < m_Abilities.Count; i++)
		{
			using (ProfileScope.New("Tick Ability"))
			{
				m_Abilities[i].Tick();
			}
		}
		m_Abilities.RemoveAll(delegate(AbilityExecutionProcess p)
		{
			if (p.IsEnded)
			{
				bool num = p.Context?.Caster.IsInCombat ?? true;
				p.Dispose();
				if (num)
				{
					Game.Instance.PlayerInputInCombatController.RequestUnlockPlayerInput();
				}
				return true;
			}
			return false;
		});
	}

	void IControllerStop.OnStop()
	{
		DetachAll();
		m_Enabled = false;
	}
}
