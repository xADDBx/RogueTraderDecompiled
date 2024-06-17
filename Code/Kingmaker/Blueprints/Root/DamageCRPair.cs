using System;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class DamageCRPair
{
	[ArrayElementNameProvider]
	public int CR;

	[SerializeField]
	public ContextValue Damage;
}
