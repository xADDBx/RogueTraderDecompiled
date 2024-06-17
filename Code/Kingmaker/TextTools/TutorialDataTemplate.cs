using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.QA;
using Kingmaker.TextTools.Base;
using Kingmaker.Tutorial;
using UnityEngine;

namespace Kingmaker.TextTools;

public class TutorialDataTemplate : TextTemplate
{
	public override int MinParameters => 1;

	public override int MaxParameters => 1;

	public override int Balance => 0;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (parameters.Count < 1)
		{
			PFLog.Default.Error("TutorialDataTemplate.Generate: parameter is missing");
		}
		GetValueForPlaceholder((parameters.Count > 0) ? parameters[0] : "", ContextData<TutorialContext>.Current, out var strValue, out var id, out var bold, out var color);
		if (id == null)
		{
			if (!bold)
			{
				return strValue;
			}
			return "<b>" + strValue + "</b>";
		}
		string text = "#" + ColorUtility.ToHtmlStringRGB(color);
		return "<b><color=" + text + "><link=\"" + id + "\">" + strValue + "</link></color></b>";
	}

	private static void GetValueForPlaceholder(string placeholder, TutorialContext context, [NotNull] out string strValue, [CanBeNull] out string id, out bool bold, out Color32 color)
	{
		id = null;
		bold = true;
		color = new Color32(0, 0, 0, byte.MaxValue);
		TutorialColors tutorialColors = Game.Instance.BlueprintRoot.UIConfig.TutorialColors;
		TutorialContextItem? tutorialContextItem = context?.Get(placeholder);
		if (!tutorialContextItem.HasValue)
		{
			PFLog.Default.ErrorWithReport("Missing value in TutorialContext: " + placeholder);
			strValue = "missing:" + placeholder;
			return;
		}
		TutorialContextItem value = tutorialContextItem.Value;
		if (value.Number.HasValue)
		{
			strValue = value.Number.Value.ToString();
		}
		else if (value.Text != null)
		{
			strValue = value.Text;
			bold = false;
		}
		else if (value.Entity != null)
		{
			if (value.Entity is BaseUnitEntity baseUnitEntity)
			{
				strValue = baseUnitEntity.CharacterName;
				id = "u:" + baseUnitEntity.UniqueId;
				color = tutorialColors.UnitLinkColor;
			}
			else if (value.Entity is ItemEntity itemEntity)
			{
				strValue = itemEntity.Name;
				id = "i:" + itemEntity.UniqueId;
				color = tutorialColors.ItemLinkColor;
			}
			else
			{
				strValue = "unsupported(" + value.Entity.GetType().Name + "):" + placeholder;
			}
		}
		else if (value.Fact != null)
		{
			strValue = value.Fact.Name;
			id = "f:" + value.Fact.Blueprint.AssetGuid;
			color = tutorialColors.FactLinkColor;
		}
		else if (value.Ability != null)
		{
			strValue = value.Ability.Name;
			id = "a:" + value.Ability.UniqueId;
			color = tutorialColors.AbilityLinkColor;
		}
		else
		{
			strValue = "empty:" + placeholder;
		}
	}
}
