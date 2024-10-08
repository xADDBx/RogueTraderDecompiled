using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

[KnowledgeDatabaseID("bec46135ed494f838087b9462878e6c7")]
public class SectorMapRumourView : EntityViewBase
{
	private bool m_VisibilityFromFilter = true;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintQuestObjectiveReference m_Blueprint;

	public bool HasParent { get; set; }

	public BlueprintQuestObjective Blueprint => m_Blueprint.Get();

	public new SectorMapRumourEntity Data => (SectorMapRumourEntity)base.Data;

	public override bool CreatesDataOnLoad => true;

	public override Entity CreateEntityData(bool load)
	{
		return CreateSectorMapRumourEntityData(load);
	}

	private SectorMapRumourEntity CreateSectorMapRumourEntityData(bool load)
	{
		return Entity.Initialize(new SectorMapRumourEntity(this));
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		UpdateVisibility();
	}

	public void SetVisibilityFromFilter(bool visible)
	{
		m_VisibilityFromFilter = visible;
		UpdateVisibility();
	}

	public void UpdateVisibility()
	{
		SetVisible(m_VisibilityFromFilter && Data.IsQuestObjectiveActive);
	}
}
