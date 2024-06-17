using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartPartyWeatherBuff : BaseUnitPart, IWeatherChangeHandler, ISubscriber, IInGameHandler, ISubscriber<IEntity>, IAreaHandler, IHashable
{
	[JsonProperty]
	private Buff m_LastBuff;

	public void OnWeatherChange()
	{
	}

	public void HandleObjectInGameChanged()
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (base.Owner == entity && base.Owner.IsInGame)
		{
			OnWeatherChange();
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		OnWeatherChange();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<Buff>.GetHash128(m_LastBuff);
		result.Append(ref val2);
		return result;
	}
}
