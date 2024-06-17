using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("cbf0304f339f4ac2880398143a2704ca")]
public class BlueprintPointOfInterestGroundOperation : BlueprintPointOfInterest
{
	[SerializeField]
	private BlueprintAreaEnterPointReference m_AreaEnterPoint;

	[Tooltip("Player can't go directly to this area, it is for checking quest objective area")]
	[SerializeField]
	private BlueprintAreaReference m_AdditionalArea;

	[SerializeField]
	private AutoSaveMode m_AutoSaveMode = AutoSaveMode.BeforeExit;

	[SerializeField]
	private List<BlueprintUnitReference> m_RequiredCompanions;

	public BlueprintAreaEnterPoint AreaEnterPoint => m_AreaEnterPoint?.Get();

	public BlueprintArea AdditionalArea => m_AdditionalArea;

	public AutoSaveMode AutoSaveMode => m_AutoSaveMode;

	public List<BlueprintUnit> RequiredCompanions => m_RequiredCompanions?.Dereference()?.EmptyIfNull().ToList();
}
