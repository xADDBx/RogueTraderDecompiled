using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Code.UI.Common;

public static class UIUtilitySpaceColonization
{
	public static string GetColonizationInformation(Colony colony)
	{
		string text = UIStrings.Instance.SystemMap.PlanetColonized.Text + " : " + colony.Planet.Name;
		ColonyProject colonyProject = Enumerable.FirstOrDefault(colony.Projects, (ColonyProject p) => !p.IsFinished);
		string text2 = string.Empty;
		if (colonyProject != null)
		{
			int num = (Game.Instance.TimeController.GameTime - colonyProject.StartTime).TotalSegments();
			int segmentsToBuild = colonyProject.Blueprint.SegmentsToBuild;
			text2 = Environment.NewLine + UIStrings.Instance.ColonyProjectsTexts.ProjectName.Text + " : " + colonyProject.Blueprint.Name + Environment.NewLine + string.Format(UIStrings.Instance.ColonyProjectsTexts.BuildingInProgress.Text, num / segmentsToBuild * 100);
		}
		List<BlueprintColonyEvent> startedEvents = colony.StartedEvents;
		BlueprintColonyEvent blueprintColonyEvent = (startedEvents.Empty() ? null : startedEvents[0]);
		string text3 = string.Empty;
		if (blueprintColonyEvent != null)
		{
			string text4 = ((!blueprintColonyEvent.Name.IsNullOrEmpty()) ? blueprintColonyEvent.Name : ((string)UIStrings.Instance.QuesJournalTexts.NoData));
			text3 = Environment.NewLine + UIStrings.Instance.ColonyProjectsTexts.EventWaitingToStart.Text + " : " + text4;
		}
		List<BlueprintDialog> startedChronicles = colony.StartedChronicles;
		string text5 = string.Empty;
		if (startedChronicles != null && !startedChronicles.Empty())
		{
			text5 = Environment.NewLine + UIStrings.Instance.ColonyProjectsTexts.NewChronicle.Text;
		}
		return text + text2 + text3 + text5;
	}
}
