using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("851d4a00ba74be04bb3229be0c6a7b6c")]
public class BlueprintMythicsSettings : BlueprintScriptableObject
{
	[SerializeField]
	[FormerlySerializedAs("_mythicsInfos")]
	private BlueprintMythicInfoReference[] m_MythicsInfos;

	[SerializeField]
	private MythicAlignment[] m_MythicAlignments;

	public IReadOnlyList<BlueprintMythicInfoReference> MythicsInfos => m_MythicsInfos;

	public bool IsMythicsSatisfied(Mythic mythic)
	{
		BlueprintMythicInfo blueprintMythicInfo = m_MythicsInfos.Dereference().FirstOrDefault((BlueprintMythicInfo x) => x.Mythic == mythic);
		if (blueprintMythicInfo != null)
		{
			return blueprintMythicInfo.IsSatisfied;
		}
		UberDebug.LogError($"[DialogError] Not found MythicInfo for: {mythic}. Return true. BlueprintMythicsSettings: {name} [{AssetGuid}]");
		return true;
	}

	public string ApplyMythicTextDecorator(string text, Mythic mythic)
	{
		BlueprintMythicInfo blueprintMythicInfo = m_MythicsInfos.Dereference().FirstOrDefault((BlueprintMythicInfo x) => x.Mythic == mythic);
		if (blueprintMythicInfo != null)
		{
			return blueprintMythicInfo.ApplyTextDecorator(text);
		}
		return text;
	}

	public AlignmentMaskType GetMythicAlignments(BlueprintCharacterClass mythic)
	{
		return m_MythicAlignments.FindOrDefault((MythicAlignment m) => m.Mythic.Get() == mythic)?.Alignment ?? AlignmentMaskType.Any;
	}

	public bool IsMythicClassUnlocked(BlueprintCharacterClass mythicClass)
	{
		return false;
	}
}
