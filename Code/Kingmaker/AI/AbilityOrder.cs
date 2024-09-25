using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.AI;

[Serializable]
public class AbilityOrder
{
	[SerializeField]
	[HideInInspector]
	private AbilitySourceWrapper[] order;

	public AbilitySourceWrapper[] Order => order.ToArray();

	public AbilityOrder()
	{
		order = Array.Empty<AbilitySourceWrapper>();
	}

	public AbilityOrder(AbilityOrder other)
	{
		order = other.Order.ToArray();
	}

	public AbilityOrder(ICollection<AbilitySourceWrapper> abilities)
	{
		order = abilities.ToArray();
	}
}
