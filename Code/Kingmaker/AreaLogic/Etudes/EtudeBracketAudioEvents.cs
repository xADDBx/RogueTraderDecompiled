using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Sound;
using Kingmaker.Visual.Sound;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("8aa8209ddf5f57e408da1c1ae7251392")]
public class EtudeBracketAudioEvents : EtudeBracketTrigger, IHashable
{
	public AkEventReference[] OnEtudeStart;

	public AkEventReference[] OnEtudeStop;

	public override bool RequireLinkedArea => true;

	protected override void OnExit()
	{
		AkEventReference[] onEtudeStop = OnEtudeStop;
		for (int i = 0; i < onEtudeStop.Length; i++)
		{
			onEtudeStop[i].Post(SoundState.Get2DSoundObject());
		}
	}

	protected override void OnEnter()
	{
		AkEventReference[] onEtudeStart = OnEtudeStart;
		for (int i = 0; i < onEtudeStart.Length; i++)
		{
			onEtudeStart[i].Post(SoundState.Get2DSoundObject());
		}
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
