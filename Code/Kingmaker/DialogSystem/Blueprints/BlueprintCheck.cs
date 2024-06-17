using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.QA;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("ab803aaa7a173f84e9172870c15e7493")]
public class BlueprintCheck : BlueprintCueBase
{
	public StatType Type;

	public int DC;

	public bool Hidden;

	public DCModifier[] DCModifiers = new DCModifier[0];

	[SerializeField]
	[FormerlySerializedAs("Success")]
	private BlueprintCueBaseReference m_Success;

	[SerializeField]
	[FormerlySerializedAs("Fail")]
	private BlueprintCueBaseReference m_Fail;

	public ActionList OnCheckSuccessActions;

	public ActionList OnCheckFailActions;

	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_UnitEvaluator;

	public BlueprintCueBase Success => m_Success?.Get();

	public BlueprintCueBase Fail => m_Fail?.Get();

	public int GetDC()
	{
		int num = DC;
		if (DCModifiers != null)
		{
			DCModifier[] dCModifiers = DCModifiers;
			foreach (DCModifier dCModifier in dCModifiers)
			{
				if (dCModifier != null && dCModifier.Conditions != null && dCModifier.Conditions.Check())
				{
					num += dCModifier.Mod;
				}
			}
		}
		return num;
	}

	public BaseUnitEntity GetTargetUnit()
	{
		AbstractUnitEntity value = null;
		if (m_UnitEvaluator != null && !m_UnitEvaluator.TryGetValue(out value))
		{
			PFLog.Default.ErrorWithReport(" BlueprintCheck {0}, AssetGUID {1}: Unit specifed but not found.", name, AssetGuid);
		}
		return value as BaseUnitEntity;
	}
}
