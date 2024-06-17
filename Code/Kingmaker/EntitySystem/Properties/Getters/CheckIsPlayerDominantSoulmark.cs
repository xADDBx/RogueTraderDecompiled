using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("dfb7daf626ba4548a64215686e8a6187")]
public class CheckIsPlayerDominantSoulmark : Condition
{
	[SerializeField]
	private SoulMarkDirection m_Direction;

	protected override string GetConditionCaption()
	{
		return "Check if Soulmark is dominant";
	}

	protected override bool CheckCondition()
	{
		if (m_Direction == SoulMarkDirection.Reason)
		{
			PFLog.Default.Error("CheckIsPlayerDominantSoulmark: Cannot compare Reason to other Soulmarks");
			return false;
		}
		SoulMark[] array = new SoulMark[3]
		{
			SoulMarkShiftExtension.GetSoulMark(SoulMarkDirection.Faith),
			SoulMarkShiftExtension.GetSoulMark(SoulMarkDirection.Hope),
			SoulMarkShiftExtension.GetSoulMark(SoulMarkDirection.Corruption)
		};
		SoulMark soulMark = array[0];
		SoulMark soulMark2 = null;
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].Rank == soulMark.Rank)
			{
				soulMark2 = array[i];
			}
			if (array[i].Rank > soulMark.Rank)
			{
				soulMark2 = null;
				soulMark = array[i];
			}
		}
		if (soulMark.GetSoulMarkDirection() != m_Direction)
		{
			return false;
		}
		if (soulMark2 != null)
		{
			PFLog.Default.Error("CheckIsPlayerDominantSoulmark: there're two equally dominant Soulmark. Cannot decide, return false");
			return false;
		}
		return true;
	}
}
