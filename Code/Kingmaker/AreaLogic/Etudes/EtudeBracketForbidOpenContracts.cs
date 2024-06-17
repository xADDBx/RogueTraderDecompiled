using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("e9789e02cf4144c7a337fd494b44b67c")]
public class EtudeBracketForbidOpenContracts : EtudeBracketTrigger, IHashable
{
	protected override void OnEnter()
	{
		Game.Instance.Player.CannotAccessContracts.Retain();
		EventBus.RaiseEvent(delegate(ICanAccessContractsHandler h)
		{
			h.HandleCanAccessContractsChanged();
		});
	}

	protected override void OnExit()
	{
		Game.Instance.Player.CannotAccessContracts.Release();
		EventBus.RaiseEvent(delegate(ICanAccessContractsHandler h)
		{
			h.HandleCanAccessContractsChanged();
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
