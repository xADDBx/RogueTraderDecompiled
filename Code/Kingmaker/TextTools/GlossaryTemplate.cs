using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.TextTools;

public class GlossaryTemplate : TextTemplate
{
	public override int MinParameters => 1;

	public override int MaxParameters => 1;

	public override int Balance => 1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (parameters.Count < 1)
		{
			PFLog.Default.Error("UITemplate.Generate: parameter is missing");
		}
		string text = ((parameters.Count > 0) ? parameters[0] : "");
		GlossaryEntry entry = GlossaryHolder.GetEntry(text);
		if (entry == null)
		{
			return "<unknown key>";
		}
		string text2 = entry.Name;
		string text3 = "ui:" + text;
		TutorialColors tutorialColors = Game.Instance.BlueprintRoot.UIConfig.TutorialColors;
		string text4 = "#" + ColorUtility.ToHtmlStringRGB(tutorialColors.UILinkColor);
		return "<b><color=" + text4 + "><link=\"" + text3 + "\">" + text2 + "</link></color></b>";
	}
}
