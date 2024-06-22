using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Sound;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("fe3b6d4ccbab402bb6f325df58a02b48")]
public class EtudeBracketEnsureAudio : EtudeBracketTrigger, IHashable
{
	public AudioFilePackagesSettings.AudioChunk Chunk;

	protected override void OnEnter()
	{
		AudioFilePackagesSettings.Instance.LoadPackagesChunk(Chunk);
		AudioFilePackagesSettings.Instance.LoadBanksChunk(Chunk);
	}

	protected override void OnExit()
	{
		AudioFilePackagesSettings.Instance.UnloadBanksChunk(Chunk);
		AudioFilePackagesSettings.Instance.UnloadPackagesChunk(Chunk);
	}

	protected override void OnDispose()
	{
		if (!EtudeBracketTrigger.Etude.IsCompleted && !EtudeBracketTrigger.Etude.CompletionInProgress)
		{
			OnExit();
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
