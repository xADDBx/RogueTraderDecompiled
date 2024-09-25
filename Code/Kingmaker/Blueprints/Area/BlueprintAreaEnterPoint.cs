using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[TypeId("168173fb7e2cd844d86be7f426467d07")]
public class BlueprintAreaEnterPoint : BlueprintScriptableObject
{
	[SerializeField]
	private BlueprintAreaReference m_Area;

	[SerializeField]
	private BlueprintAreaPartReference m_AreaPart;

	[CanBeNull]
	[SerializeField]
	private List<LocalizedString> m_TooltipList;

	[SerializeField]
	private LocalizedString m_Tooltip;

	public Sprite Icon;

	public Sprite HoverIcon;

	public string TooltipDescription => m_Tooltip;

	public BlueprintArea Area
	{
		get
		{
			return m_Area.Get();
		}
		set
		{
			m_Area = value.ToReference<BlueprintAreaReference>();
		}
	}

	public BlueprintAreaPart AreaPart
	{
		get
		{
			return m_AreaPart.Get();
		}
		set
		{
			m_AreaPart = value.ToReference<BlueprintAreaPartReference>();
		}
	}

	[CanBeNull]
	public LocalizedString Tooltip(int index)
	{
		if (m_TooltipList == null || m_TooltipList.Count == 0)
		{
			return m_Tooltip;
		}
		int num = m_TooltipList.Count - 1;
		if (index > num)
		{
			PFLog.System.Error("BlueprintAreaEnterPoint.Tooltip: requested index > maxIndex");
			index = num;
		}
		return m_TooltipList[index];
	}
}
