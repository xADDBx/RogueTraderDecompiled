using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Collections;

[TypeId("21d74373cc7a6654490abb3292793018")]
public class BuffCollection : BlueprintScriptableObject, IBlueprintScanner
{
	public bool CheckHidden;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("BuffList")]
	private BlueprintBuffReference[] m_BuffList;

	public ReferenceArrayProxy<BlueprintBuff> BuffList
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffList = m_BuffList;
			return buffList;
		}
	}

	public void Scan()
	{
	}
}
