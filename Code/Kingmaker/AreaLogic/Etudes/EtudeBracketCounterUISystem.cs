using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[TypeId("eb4116fd26774f738240221a5c5d39a4")]
public class EtudeBracketCounterUISystem : EtudeBracketTrigger, IHashable
{
	public EtudeUICounterSystemTypes Type;

	protected override void OnEnter()
	{
		Show();
	}

	protected override void OnExit()
	{
		Hide();
	}

	protected override void OnResume()
	{
		Show();
	}

	private void Show()
	{
		EventBus.RaiseEvent(delegate(IEtudeCounterSystemHandler h)
		{
			h.ShowEtudeCounterSystem(Type);
		});
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IEtudeCounterSystemHandler h)
		{
			h.HideEtudeCounterSystem(Type);
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
