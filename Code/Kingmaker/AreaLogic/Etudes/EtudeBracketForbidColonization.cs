using Core.Cheats;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("3a97e5efd5d74e74893a0c1ed7296aa1")]
public class EtudeBracketForbidColonization : EtudeBracketTrigger, IHashable
{
	protected override void OnEnter()
	{
		Game.Instance.Player.ColoniesState.ForbidColonization.Retain();
		EventBus.RaiseEvent(delegate(ICanAccessColonizationHandler h)
		{
			h.HandleCanAccessColonization();
		});
	}

	protected override void OnExit()
	{
		Game.Instance.Player.ColoniesState.ForbidColonization.Release();
		EventBus.RaiseEvent(delegate(ICanAccessColonizationHandler h)
		{
			h.HandleCanAccessColonization();
		});
	}

	[Cheat(Name = "change_colonization_access")]
	public static void ChangeForbidColonization()
	{
		if ((bool)Game.Instance.Player.ColoniesState.ForbidColonization)
		{
			Game.Instance.Player.ColoniesState.ForbidColonization.Release();
		}
		else
		{
			Game.Instance.Player.ColoniesState.ForbidColonization.Retain();
		}
		EventBus.RaiseEvent(delegate(ICanAccessColonizationHandler h)
		{
			h.HandleCanAccessColonization();
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
