using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem.Entities.Base;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class AreaCROverrideManager : EntityPart<Player>, IHashable
{
	private readonly Dictionary<string, List<OverrideAreaCR>> m_OverridenCR = new Dictionary<string, List<OverrideAreaCR>>();

	public bool TryGetValue(string areaPartAssetGuid, out int value)
	{
		if (!m_OverridenCR.TryGetValue(areaPartAssetGuid, out List<OverrideAreaCR> value2))
		{
			value = 0;
			return false;
		}
		if (value2.Count == 0)
		{
			value = 0;
			return false;
		}
		int num = 0;
		foreach (OverrideAreaCR item in value2)
		{
			num = Mathf.Max(num, item.NewCR);
		}
		value = num;
		return true;
	}

	public bool Contains(string areaPartAssetGuid)
	{
		if (!m_OverridenCR.TryGetValue(areaPartAssetGuid, out List<OverrideAreaCR> value))
		{
			return false;
		}
		if (value.Count == 0)
		{
			return false;
		}
		return true;
	}

	public void Add(OverrideAreaCR component)
	{
		if (m_OverridenCR.TryGetValue(component.LinkedAreaPart.AssetGuid, out List<OverrideAreaCR> value))
		{
			if (value.Contains(component))
			{
				PFLog.Etudes.Error($"AreaCROverrideManager already contains {component.OwnerBlueprint.name} overrides CR to {component.NewCR} in area {component.LinkedAreaPart.name} -- ignoring!");
				return;
			}
		}
		else
		{
			value = new List<OverrideAreaCR>();
			m_OverridenCR.Add(component.LinkedAreaPart.AssetGuid, value);
		}
		value.Add(component);
		PFLog.Etudes.Log($"{component.OwnerBlueprint.name} overrides CR to {component.NewCR} in area {component.LinkedAreaPart.name}");
	}

	public void Remove(OverrideAreaCR component)
	{
		if (!m_OverridenCR.TryGetValue(component.LinkedAreaPart.AssetGuid, out List<OverrideAreaCR> value))
		{
			PFLog.Etudes.Error("There's no any CR overrides in area " + component.LinkedAreaPart.name + "!");
		}
		else if (!value.Remove(component))
		{
			PFLog.Etudes.Error($"There's no CR overrides in area {component.LinkedAreaPart.name} by {component.OwnerBlueprint.name} to {component.NewCR}!");
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
