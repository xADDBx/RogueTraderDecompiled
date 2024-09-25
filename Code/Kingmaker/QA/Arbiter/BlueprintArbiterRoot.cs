using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

[TypeId("72d8a7aaec384eafb0945d195670dc84")]
public class BlueprintArbiterRoot : BlueprintScriptableObject
{
	public class BlueprintArbiterRootReference : BlueprintReference<BlueprintArbiterRoot>
	{
		public BlueprintArbiterRootReference()
		{
			guid = "fa749e5dbded486ebfe11f028d913b96";
		}
	}

	[SerializeField]
	private string ProjectId = "unknown";

	public Vector2Int Resolution = new Vector2Int(1920, 1080);

	public int CombatTimeout = 1200;

	public int UnitCombatTurnTimeout = 60;

	public SceneReference[] IgnoreScenesInReport;

	public string[] IgnoreScenesInReportByFilter;

	private static BlueprintReference<BlueprintArbiterRoot> RootRef { get; } = new BlueprintArbiterRootReference();


	public static BlueprintArbiterRoot Instance => RootRef.Get();

	public string Project => ProjectId;

	public bool IsSceneIgnoredInReport(SceneReference sceneReference)
	{
		if (!IgnoreScenesInReport.Contains(sceneReference))
		{
			return IgnoreScenesInReportByFilter.Any((string x) => sceneReference.SceneName.Contains(x));
		}
		return true;
	}
}
