using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("7d9dc37dd50d49e1bfe8001119168464")]
public class EtudeIgnorePartyEncumbrance : EtudeBracketTrigger, IHashable
{
	protected override void OnEnter()
	{
		Game.Instance.LoadedAreaState.Settings.IgnorePartyEncumbrance.Retain();
	}

	protected override void OnExit()
	{
		Game.Instance.LoadedAreaState.Settings.IgnorePartyEncumbrance.Release();
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
