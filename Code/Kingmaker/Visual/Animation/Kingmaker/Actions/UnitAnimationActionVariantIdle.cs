using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationVariantIdle", menuName = "Animation Manager/Actions/Unit Variant (noncombat) Idle")]
public class UnitAnimationActionVariantIdle : UnitAnimationAction
{
	[Serializable]
	public class ClassOverrideData
	{
		[SerializeField]
		[FormerlySerializedAs("Class")]
		private BlueprintCharacterClassReference m_Class;

		[Range(0f, 1f)]
		public float Chance = 1f;

		[ValidateNoNullEntries]
		public List<AnimationClipWrapper> ClipWrappers;

		public BlueprintCharacterClass Class => m_Class?.Get();
	}

	[Serializable]
	public class RaceOverrideData
	{
		[SerializeField]
		[FormerlySerializedAs("Race")]
		private BlueprintRaceReference m_Race;

		[Range(0f, 1f)]
		public float Chance = 1f;

		[ValidateNoNullEntries]
		public List<AnimationClipWrapper> ClipWrappers;

		public BlueprintRace Race => m_Race?.Get();
	}

	[SerializeField]
	private bool m_ForDollRoom;

	[SerializeField]
	[ValidateNoNullEntries]
	private List<AnimationClipWrapper> m_ClipWrappers;

	public TimedProbabilityCurve RetriggerProbability;

	public bool PlayOnNPC;

	public ClassOverrideData[] ClassOverrides;

	public RaceOverrideData[] RaceOverrides;

	public override UnitAnimationType Type
	{
		get
		{
			if (!m_ForDollRoom)
			{
				return UnitAnimationType.VariantIdle;
			}
			return UnitAnimationType.DollRoomVariantIdle;
		}
	}

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_ClipWrappers;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		List<AnimationClipWrapper> clipWrappers = m_ClipWrappers;
		List<AnimationClipWrapper> list = null;
		List<AnimationClipWrapper> list2 = null;
		if (ClassOverrides != null && ClassOverrides.Length != 0)
		{
			BlueprintCharacterClass @class = handle.UnitClass;
			ClassOverrideData classOverrideData = ClassOverrides.FirstOrDefault((ClassOverrideData c) => c.Class == @class);
			if (classOverrideData?.ClipWrappers != null && classOverrideData.ClipWrappers.Count > 0 && handle.Manager.StatefulRandom.value <= classOverrideData.Chance)
			{
				list = classOverrideData.ClipWrappers;
			}
		}
		if (RaceOverrides != null && RaceOverrides.Length != 0 && handle.UnitRace != null)
		{
			RaceOverrideData raceOverrideData = RaceOverrides.FirstOrDefault((RaceOverrideData c) => c.Race == handle.UnitRace);
			if (raceOverrideData?.ClipWrappers != null && raceOverrideData.ClipWrappers.Count > 0 && handle.Manager.StatefulRandom.value <= raceOverrideData.Chance)
			{
				list2 = raceOverrideData.ClipWrappers;
			}
		}
		if (handle.Unit?.AnimationManager?.CustomIdleWrappers != null)
		{
			AbstractUnitEntityView unit = handle.Unit;
			if ((object)unit != null && unit.AnimationManager?.CustomIdleWrappers.Count > 0)
			{
				clipWrappers = handle.Unit?.AnimationManager?.CustomIdleWrappers;
				goto IL_020f;
			}
		}
		List<AnimationClipWrapper> list3 = ((!(handle.Manager.StatefulRandom.value > 0.5f)) ? ((list != null && list.Count > 0) ? list : list2) : ((list2 != null && list2.Count > 0) ? list2 : list));
		clipWrappers = ((list3 != null && list3.Count > 0) ? list3 : clipWrappers);
		goto IL_020f;
		IL_020f:
		if (clipWrappers == null || clipWrappers.Count == 0)
		{
			handle.Variant = -1;
			handle.Release();
		}
		else
		{
			handle.Variant = handle.Manager.StatefulRandom.Range(0, clipWrappers.Count);
			handle.StartClip(clipWrappers[handle.Variant]);
		}
	}
}
