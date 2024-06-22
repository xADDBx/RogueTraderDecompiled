using System;
using Kingmaker.AreaLogic.TimeSurvival;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Controllers.SpaceCombat;

public class ExitSpaceCombatController : BaseUnitController, IControllerStart, IController
{
	private static TimeSpan s_EndSpaceCombatTimeout = 2.Seconds();

	private static bool s_ExitSpaceCombatRequested;

	private TimeSpan? m_EndSpaceCombatTime;

	private TimeSurvival m_TimeSurvival;

	public void OnStart()
	{
		s_ExitSpaceCombatRequested = false;
		m_EndSpaceCombatTime = null;
		m_TimeSurvival = Game.Instance.CurrentlyLoadedArea.GetComponent<TimeSurvival>();
	}

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		if (unit != Game.Instance.Player.PlayerShip || unit.IsInCombat || s_ExitSpaceCombatRequested)
		{
			return;
		}
		TimeSurvival timeSurvival = m_TimeSurvival;
		if (timeSurvival != null && !timeSurvival.UnlimitedTime && timeSurvival.RoundsLeft > 0)
		{
			return;
		}
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		if (!m_EndSpaceCombatTime.HasValue)
		{
			m_EndSpaceCombatTime = gameTime + s_EndSpaceCombatTimeout;
		}
		else if (m_EndSpaceCombatTime <= gameTime)
		{
			HandlePlayerStarshipProgression();
			EventBus.RaiseEvent(delegate(IEndSpaceCombatHandler h)
			{
				h.HandleEndSpaceCombat();
			});
			s_ExitSpaceCombatRequested = true;
			m_EndSpaceCombatTime = null;
			m_TimeSurvival = null;
		}
	}

	public static void ExitSpaceCombat(bool forceOpenVoidshipUpgrade)
	{
		Game.Instance.Player.IsForceOpenVoidshipUpgrade = forceOpenVoidshipUpgrade;
		s_ExitSpaceCombatRequested = false;
		EndSpaceCombat();
		EventBus.RaiseEvent(delegate(IExitSpaceCombatHandler h)
		{
			h.HandleExitSpaceCombat();
		});
	}

	private static void EndSpaceCombat()
	{
		EndCooldowns();
		RepairPlayerStarship();
		ReturnToStarSystemMap();
	}

	private static void RepairPlayerStarship()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		PartUnitCombatState required = playerShip.Parts.GetRequired<PartUnitCombatState>();
		playerShip.Health.HealDamage(required.DamageReceivedInCurrentFight / 2);
		int num = (int)Math.Round((float)playerShip.Health.MaxHitPoints * 0.25f);
		if (playerShip.Health.HitPointsLeft < num)
		{
			playerShip.Health.SetHitPointsLeft(num);
		}
		playerShip.Shields.DeactivateShields();
		playerShip.Shields.ActivateShields();
		playerShip.Crew.RestoreCrew();
	}

	private static void HandlePlayerStarshipProgression()
	{
		StarshipEntity ship = Game.Instance.Player.PlayerShip;
		PartUnitProgression shipProgression = ship.Progression;
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintSpaceCombatArea spaceCombatArea = currentlyLoadedArea as BlueprintSpaceCombatArea;
		if (spaceCombatArea != null)
		{
			shipProgression.GainExperience(spaceCombatArea.AdditionalExperience);
			EventBus.RaiseEvent((IStarshipEntity)ship, (Action<IUISpaceCombatExperienceGainedPerAreaHandler>)delegate(IUISpaceCombatExperienceGainedPerAreaHandler o)
			{
				o.HandlerOnSpaceCombatExperienceGainedPerArea(spaceCombatArea.AdditionalExperience);
			}, isCheckRuntime: true);
			EventBus.RaiseEvent((IStarshipEntity)ship, (Action<IStarshipExpToNextLevelHandler>)delegate(IStarshipExpToNextLevelHandler h)
			{
				h.HandleStarshipExpToNextLevel(shipProgression.ExperienceLevel, ship.StarshipProgression.ExpToNextLevel, spaceCombatArea.AdditionalExperience);
			}, isCheckRuntime: true);
		}
	}

	private static void EndCooldowns()
	{
		Game.Instance.Player.PlayerShip.AbilityCooldowns.Clear();
	}

	private static void ReturnToStarSystemMap()
	{
		BlueprintArea previousVisitedArea = Game.Instance.Player.PreviousVisitedArea;
		Vector3? mapCoord = Game.Instance.Player.LastPositionOnPreviousVisitedArea;
		if (previousVisitedArea == null)
		{
			return;
		}
		BlueprintAreaEnterPoint enterPoint = previousVisitedArea.DefaultPreset.EnterPoint;
		Game.Instance.LoadArea(enterPoint, AutoSaveMode.None, delegate
		{
			if (mapCoord.HasValue)
			{
				Game.Instance.Player.PlayerShip.Position = mapCoord.Value;
			}
		});
	}
}
