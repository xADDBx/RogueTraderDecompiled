using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.SystemMechanics;

[TypeId("556d56ac73b44e5e844554a43f6c2d8b")]
public class UnitConditionBuffsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<UnitConditionBuffsRoot>
	{
	}

	[Serializable]
	public class Entry
	{
		public UnitCondition Type;

		[SerializeField]
		[ValidateNotNull]
		private BlueprintBuffReference m_Buff;

		public BlueprintBuff Buff => m_Buff;
	}

	public Entry[] MarkerBuffs = Array.Empty<Entry>();

	public Entry[] ControlBuffs = Array.Empty<Entry>();

	[CanBeNull]
	public BlueprintBuff GetMarker(UnitCondition condition)
	{
		return MarkerBuffs?.FirstItem((Entry i) => i.Type == condition)?.Buff;
	}

	[CanBeNull]
	public BlueprintBuff GetControl(UnitCondition condition)
	{
		return ControlBuffs?.FirstItem((Entry i) => i.Type == condition)?.Buff;
	}
}
