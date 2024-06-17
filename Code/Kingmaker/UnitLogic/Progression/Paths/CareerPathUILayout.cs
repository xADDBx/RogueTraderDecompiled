using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Paths;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintCareerPath))]
[TypeId("479944fd87d44924a20a738305939a23")]
public class CareerPathUILayout : BlueprintComponent
{
	public enum RankEntryLayoutType
	{
		Horizontal,
		Vertical
	}

	[Serializable]
	public class RankEntryLayout
	{
		public Vector2 Position;

		public RankEntryLayoutType LayoutType;

		public bool IsReversed;
	}

	[InfoBox("Ranks must be equal to RankEntries.Length")]
	public RankEntryLayout[] RankEntries = new RankEntryLayout[0];
}
