using System;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.View.Roaming;

public interface IRoamingPoint
{
	Vector3 Position { get; }

	float? Orientation { get; }

	TimeSpan SelectIdleTime(StatefulRandom random);

	[CanBeNull]
	Cutscene SelectCutscene(StatefulRandom random);

	[CanBeNull]
	IRoamingPoint SelectNextPoint(StatefulRandom random);

	[CanBeNull]
	IRoamingPoint SelectPrevPoint(StatefulRandom random);
}
