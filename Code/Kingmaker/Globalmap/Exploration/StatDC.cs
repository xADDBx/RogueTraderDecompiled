using System;
using Kingmaker.EntitySystem.Stats.Base;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

[Serializable]
public class StatDC
{
	[SerializeField]
	public StatType Stat;

	[Tooltip("For penalty use negative DC")]
	[SerializeField]
	public int DC;
}
