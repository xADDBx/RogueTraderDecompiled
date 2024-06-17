using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("4522987d49ef4d28868f777783154e2b")]
public class EtudeBracketOverrideWeatherProfile : EtudeBracketTrigger, IHashable
{
	[SerializeField]
	private WeatherProfileExtended m_WeatherProfile;

	private EtudeBracketGameModeWaiter m_GameModeWaiter;

	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		m_GameModeWaiter = new EtudeBracketGameModeWaiter(delegate
		{
			VFXWeatherSystem.Instance?.OverrideProfile(overrideProfile: true, m_WeatherProfile);
		}, delegate
		{
			VFXWeatherSystem.Instance?.OverrideProfile(overrideProfile: false, null);
		});
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	protected override void OnExit()
	{
		m_GameModeWaiter?.Dispose();
		m_GameModeWaiter = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
