using System;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.View.Roaming;

[Serializable]
public class NextWaypointEntry : IWeighted
{
	public RoamingWaypointView Waypoint;

	[SerializeField]
	private float m_Weight = 1f;

	public float Weight => m_Weight;
}
