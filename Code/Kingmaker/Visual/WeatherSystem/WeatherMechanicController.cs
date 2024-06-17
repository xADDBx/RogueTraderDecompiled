using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.WeatherSystem;

public class WeatherMechanicController : IWeatherEntityController, IDisposable, IWeatherChangeHandler, ISubscriber, IAreaHandler, IPartyLeaveAreaHandler, ITeleportHandler
{
	protected WeatherMechanicSettings m_Settings;

	private BaseUnitEntity Actor;

	public WeatherMechanicController(WeatherMechanicSettings settings, Transform root)
	{
		m_Settings = settings;
		EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		UpdateBuff();
	}

	public void HandlePartyLeaveArea(BlueprintArea currentArea, BlueprintAreaEnterPoint targetArea)
	{
		RemoveBuff();
	}

	public void HandlePartyTeleport(AreaEnterPoint enterPoint)
	{
		UpdateBuff();
	}

	public void OnWeatherChange()
	{
		UpdateBuff();
	}

	private void UpdateBuff()
	{
	}

	public virtual void Update(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
	}

	private void AddBuff()
	{
	}

	private void RemoveBuff()
	{
	}

	private void CastAbility()
	{
		if (Actor == null)
		{
			Actor = GetActor(m_Settings.BlueprintActor);
		}
		foreach (BaseUnitEntity target in GetTargets())
		{
			if (m_Settings.BlueprintAbility != null)
			{
				Rulebook.Trigger(new RulePerformAbility(new AbilityData(m_Settings.BlueprintAbility, Actor), target));
			}
		}
	}

	private IEnumerable<BaseUnitEntity> GetTargets()
	{
		switch (m_Settings.Target)
		{
		case WeatherAbilityTarget.All:
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
			{
				yield return allBaseAwakeUnit;
			}
			break;
		case WeatherAbilityTarget.AllParty:
			foreach (BaseUnitEntity item in Game.Instance.Player.Party)
			{
				yield return item;
			}
			break;
		case WeatherAbilityTarget.AllPartyAndPets:
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				yield return partyAndPet;
			}
			break;
		case WeatherAbilityTarget.AllEnemy:
		{
			IEnumerable<BaseUnitEntity> enumerable = Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity _unit) => _unit.Faction.IsPlayerEnemy && _unit != Game.Instance.Player.MainCharacterEntity);
			foreach (BaseUnitEntity item2 in enumerable)
			{
				yield return item2;
			}
			break;
		}
		case WeatherAbilityTarget.Random:
			yield return Game.Instance.State.AllBaseAwakeUnits[(int)((double)(PFStatefulRandom.Weather.value * (float)Game.Instance.State.AllBaseAwakeUnits.Count) * 0.9999)];
			break;
		case WeatherAbilityTarget.RandomParty:
			yield return Game.Instance.Player.Party[(int)((double)(PFStatefulRandom.Weather.value * (float)Game.Instance.Player.Party.Count) * 0.9999)];
			break;
		case WeatherAbilityTarget.RandomPartyAndPets:
			yield return Game.Instance.Player.PartyAndPets[(int)((double)(PFStatefulRandom.Weather.value * (float)Game.Instance.Player.PartyAndPets.Count) * 0.9999)];
			break;
		case WeatherAbilityTarget.RandomEnemy:
		{
			List<BaseUnitEntity> list = Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity _unit) => _unit.Faction.IsPlayerEnemy && _unit != Game.Instance.Player.MainCharacterEntity).ToTempList();
			yield return list[(int)((double)(PFStatefulRandom.Weather.value * (float)list.Count) * 0.9999)];
			break;
		}
		}
	}

	private BaseUnitEntity GetActor(BlueprintUnit unit)
	{
		if (unit == null)
		{
			return null;
		}
		BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(unit, Vector3.zero, Quaternion.identity, Game.Instance.LoadedAreaState.MainState);
		baseUnitEntity.IsInGame = false;
		return baseUnitEntity;
	}
}
