using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

[KnowledgeDatabaseID("98ffbe860ca646c49d0b6c95572e2411")]
public class SectorMapRumourGroupView : EntityViewBase, IRumourObjectiveStateHandler, ISubscriber
{
	public class SectorMapRumourGroupEntity : SimpleEntity, IHashable
	{
		public readonly SectorMapRumourGroupView GroupView = new SectorMapRumourGroupView();

		public new SectorMapRumourGroupView View => (SectorMapRumourGroupView)base.View;

		public SectorMapRumourGroupEntity(EntityViewBase view)
			: base(view)
		{
		}

		protected SectorMapRumourGroupEntity(JsonConstructorMark _)
			: base(_)
		{
		}

		protected override IEntityViewBase CreateViewForData()
		{
			return null;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	private Renderer m_Renderer = new Renderer();

	private readonly HashSet<SectorMapRumourView> m_ChildComponents = new HashSet<SectorMapRumourView>();

	public HashSet<BlueprintQuestObjective> ActiveQuestObjectives = new HashSet<BlueprintQuestObjective>();

	public override List<Renderer> Renderers => new List<Renderer> { m_Renderer };

	public override bool CreatesDataOnLoad => true;

	protected override void OnEnable()
	{
		base.OnEnable();
		SectorMapRumourView[] componentsInChildren = GetComponentsInChildren<SectorMapRumourView>();
		foreach (SectorMapRumourView sectorMapRumourView in componentsInChildren)
		{
			if (sectorMapRumourView != null)
			{
				sectorMapRumourView.HasParent = true;
				m_ChildComponents.Add(sectorMapRumourView);
			}
		}
		Renderer componentInChildren = GetComponentInChildren<Renderer>(includeInactive: true);
		if (componentInChildren != null)
		{
			m_Renderer = componentInChildren;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_ChildComponents.Clear();
		m_Renderer = null;
	}

	protected override void OnDidAttachToData()
	{
		TryGetAlreadyActiveQuestObjectives();
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new SectorMapRumourGroupEntity(this));
	}

	public void HandleRumourObjectiveActiveStateChanged(BlueprintQuestObjective objective, bool isActive)
	{
		try
		{
			if (!(m_ChildComponents.FirstOrDefault((SectorMapRumourView i) => i.Blueprint == objective) == null))
			{
				if (isActive && !ActiveQuestObjectives.Contains(objective))
				{
					ActiveQuestObjectives.Add(objective);
				}
				m_Renderer.gameObject.SetActive(!ActiveQuestObjectives.Empty());
				m_Renderer.enabled = !ActiveQuestObjectives.Empty();
			}
		}
		catch (Exception arg)
		{
			PFLog.Default.Log($"SectorMapRumourGroupView: {arg}");
		}
	}

	public void TryGetAlreadyActiveQuestObjectives()
	{
		foreach (SectorMapRumourView childComponent in m_ChildComponents)
		{
			SectorMapRumourEntity data = childComponent.Data;
			if (data != null && data.IsQuestObjectiveActive && !ActiveQuestObjectives.Contains(childComponent.Blueprint))
			{
				ActiveQuestObjectives.Add(childComponent.Blueprint);
			}
		}
		GetComponentInChildren<Renderer>(includeInactive: true).gameObject.SetActive(!ActiveQuestObjectives.Empty());
		GetComponentInChildren<Renderer>(includeInactive: true).enabled = !ActiveQuestObjectives.Empty();
	}
}
