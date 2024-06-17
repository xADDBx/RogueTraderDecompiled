using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Sound;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("ba35ab260ded0054d94ab835dad46534")]
public class EtudeBracketMusic : EtudeBracketTrigger, IHashable
{
	public AkEventReference StartTheme;

	public AkEventReference StopTheme;

	public override bool RequireLinkedArea => true;

	protected override void OnExit()
	{
		LogChannel.Audio.Log("Stop etude music " + this);
	}

	protected override void OnEnter()
	{
		LogChannel.Audio.Log("Play etude music " + this);
	}

	protected override void OnResume()
	{
		LogChannel.Audio.Log("Resume etude music " + this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
