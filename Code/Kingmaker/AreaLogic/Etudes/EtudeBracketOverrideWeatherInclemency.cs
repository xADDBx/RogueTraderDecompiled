using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("26269606ecc24ace9e10929a0f8603d2")]
public class EtudeBracketOverrideWeatherInclemency : EtudeBracketTrigger, IHashable
{
	public InclemencyType Inclemency;

	protected override void OnEnter()
	{
		Game.Instance.Player.Weather.CurrentWeather = Inclemency;
		EventBus.RaiseEvent(delegate(IWeatherUpdateHandler h)
		{
			h.OnUpdateWeatherSystem(overrideWeather: true);
		});
	}

	protected override void OnExit()
	{
		EventBus.RaiseEvent(delegate(IWeatherUpdateHandler h)
		{
			h.OnUpdateWeatherSystem(overrideWeather: false);
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
