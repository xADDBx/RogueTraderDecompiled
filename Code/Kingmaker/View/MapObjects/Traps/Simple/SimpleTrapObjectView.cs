using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps.Simple;

[KnowledgeDatabaseID("3f97579788494ffdb20292bb50f3b2e7")]
public class SimpleTrapObjectView : TrapObjectView
{
	[SerializeField]
	private LocalizedString m_NameInLog;

	[SerializeField]
	[CanBeNull]
	private ActionsReference m_DisableActions;

	[SerializeField]
	[CanBeNull]
	private ActionsReference m_TrapActions;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintMechanicEntityFact.Reference[] m_Facts;

	[SerializeField]
	public SimpleTrapObjectInfo Info;

	public string NameInLog
	{
		get
		{
			if (!m_NameInLog.Text.IsNullOrEmpty())
			{
				return m_NameInLog.Text;
			}
			return "<set name in log>";
		}
	}

	[CanBeNull]
	public ActionsHolder DisableActions => m_DisableActions;

	[CanBeNull]
	public ActionsHolder TrapActions => m_TrapActions;

	public new SimpleTrapObjectData Data => (SimpleTrapObjectData)base.Data;

	public static SimpleTrapObjectView CreateView(SimpleTrapObjectData data, string uniqueId, string scriptZoneId)
	{
		if (data.View == null)
		{
			PFLog.Default.Error($"Can not create view for {typeof(SimpleTrapObjectData)} {uniqueId} because trap guides in the scene and the save do not match");
			return null;
		}
		if (data?.Info == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject(data.Name);
		gameObject.SetActive(value: false);
		SimpleTrapObjectView simpleTrapObjectView = gameObject.AddComponent<SimpleTrapObjectView>();
		ScriptZone scriptZoneTrigger = TrapObjectView.SetupViewAndFindScriptZone(simpleTrapObjectView, scriptZoneId, UnitAnimationInteractionType.DisarmTrap);
		AttachPerceptionComponent(gameObject, data);
		simpleTrapObjectView.Info = data.Info;
		simpleTrapObjectView.UniqueId = uniqueId;
		simpleTrapObjectView.Settings = new TrapObjectViewSettings
		{
			ScriptZoneTrigger = scriptZoneTrigger
		};
		gameObject.SetActive(value: true);
		return simpleTrapObjectView;
	}

	protected override TrapObjectData CreateData()
	{
		BlueprintTrapSettingsRoot blueprintTrapSettingsRoot = BlueprintRoot.Instance.BlueprintTrapSettingsRoot;
		BlueprintTrapSettings settings = GetSettings(Game.Instance.LoadedAreaState.Blueprint);
		int num = ((PFStatefulRandom.View.Range(0, 2) != 0) ? 1 : (-1));
		int disableDC = ((Info.DisableDifficulty == TrapDisableDifficulty.Easy) ? (settings.DisableDC.from + blueprintTrapSettingsRoot.EasyDisableDCDelta * num) : (settings.DisableDC.to + blueprintTrapSettingsRoot.HardDisableDCDelta * num));
		SimpleTrapObjectData simpleTrapObjectData = Entity.Initialize(new SimpleTrapObjectData(this)
		{
			Name = base.gameObject.name,
			DisableDC = disableDC,
			PerceptionDC = settings.PerceptionDC.PickRandom(),
			PerceptionRadius = (Info.OverridePerceptionRadius ? Info.PerceptionRadius : blueprintTrapSettingsRoot.DefaultPerceptionRadius),
			TrapSettings = settings
		});
		AttachPerceptionComponent(base.gameObject, simpleTrapObjectData);
		if (LinkedTrap != null)
		{
			AttachPerceptionComponent(LinkedTrap.gameObject, simpleTrapObjectData);
		}
		BlueprintMechanicEntityFact.Reference[] facts = m_Facts;
		for (int i = 0; i < facts.Length; i++)
		{
			simpleTrapObjectData.AddFact(facts[i]?.Get());
		}
		return simpleTrapObjectData;
	}

	public BlueprintTrapSettings GetSettings(BlueprintArea blueprintArea)
	{
		return BlueprintRoot.Instance.BlueprintTrapSettingsRoot.Find(blueprintArea.GetCR() + Info.AdditionalCR);
	}

	private static void AttachPerceptionComponent(GameObject gameObject, SimpleTrapObjectData data)
	{
		AwarenessCheckComponent awarenessCheckComponent = gameObject.EnsureComponent<AwarenessCheckComponent>();
		awarenessCheckComponent.SetCustomDC(data.PerceptionDC);
		awarenessCheckComponent.Difficulty = SkillCheckDifficulty.Custom;
		awarenessCheckComponent.Radius = data.PerceptionRadius;
	}
}
