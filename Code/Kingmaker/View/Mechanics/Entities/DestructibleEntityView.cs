using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.SystemMechanics;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Entities;

public class DestructibleEntityView : MapObjectView, IDestructionStagesManager
{
	[Serializable]
	private class DestructionStageSettings
	{
		public DestructionStage Type;

		public GridNavmeshModifier NavmeshModifier;
	}

	[SerializeField]
	private bool m_UseCustomBlueprint;

	[SerializeField]
	[HideIf("m_UseCustomBlueprint")]
	private StandardDestructibleObjectType m_StandardBlueprintType;

	[SerializeField]
	[ShowIf("m_UseCustomBlueprint")]
	private BlueprintDestructibleObject.Reference m_CustomBlueprint;

	public bool m_CanBeAttackedDirectly = true;

	[SerializeField]
	private DestructionStageSettings[] m_DestructionStages = new DestructionStageSettings[0];

	private new Collider[] m_Colliders;

	private DestructionStage m_DestructionStage;

	private Vector3? m_OvertipPosition;

	private Rect m_CachedBounds;

	private Vector3 m_PrevPosition;

	private bool m_ForceRecalculateBounds = true;

	public GridNavmeshModifier GridNavmeshModifier { get; private set; }

	public bool UseCustomBlueprint => m_UseCustomBlueprint;

	public bool HasCustomBlueprint
	{
		get
		{
			if (m_CustomBlueprint != null)
			{
				return !m_CustomBlueprint.IsEmpty();
			}
			return false;
		}
	}

	public BlueprintDestructibleObject Blueprint
	{
		get
		{
			if (!m_UseCustomBlueprint)
			{
				return BlueprintWarhammerRoot.Instance.DestructibleObjectsRoot.GetStandardObject(m_StandardBlueprintType);
			}
			return m_CustomBlueprint?.Get() ?? BlueprintWarhammerRoot.Instance.DestructibleObjectsRoot.GetStandardObject(m_StandardBlueprintType);
		}
	}

	public Rect Bounds => CalculateBounds();

	public new DestructibleEntity Data => (DestructibleEntity)base.Data;

	public IEnumerable<DestructionStage> Stages => m_DestructionStages.Select((DestructionStageSettings i) => i.Type);

	protected override bool IsClickable
	{
		get
		{
			if (!CanBeAttackedDirectly)
			{
				return base.IsClickable;
			}
			return true;
		}
	}

	public override bool CanBeAttackedDirectly => m_CanBeAttackedDirectly;

	public Vector3 OvertipPosition
	{
		get
		{
			if (m_OvertipPosition.HasValue)
			{
				return m_OvertipPosition.Value;
			}
			Bounds? bounds = null;
			if (m_Colliders != null)
			{
				Collider[] colliders = m_Colliders;
				foreach (Collider collider in colliders)
				{
					if (collider.enabled && collider.gameObject.activeInHierarchy)
					{
						if (!bounds.HasValue)
						{
							bounds = collider.bounds;
						}
						else
						{
							bounds.Value.Encapsulate(collider.bounds);
						}
					}
				}
			}
			Bounds valueOrDefault = bounds.GetValueOrDefault();
			if (!bounds.HasValue)
			{
				valueOrDefault = new Bounds(base.ViewTransform.position, Vector3.one);
				bounds = valueOrDefault;
			}
			m_OvertipPosition = bounds.Value.center + new Vector3(0f, bounds.Value.extents.y, 0f);
			return m_OvertipPosition.Value;
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new DestructibleEntity(this));
	}

	protected override void Awake()
	{
		base.Awake();
		if (!Application.isPlaying)
		{
			return;
		}
		m_Colliders = GetComponentsInChildren<Collider>(includeInactive: true);
		DestructionStageSettings[] destructionStages = m_DestructionStages;
		foreach (DestructionStageSettings destructionStageSettings in destructionStages)
		{
			if (destructionStageSettings.NavmeshModifier != null)
			{
				destructionStageSettings.NavmeshModifier.gameObject.SetActive(value: false);
			}
		}
	}

	protected override Color GetHighlightColor()
	{
		return MapObjectView.UIConfig.EnemyHighlightColor;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GridNavmeshModifier = this.GetComponentNonAlloc<GridNavmeshModifier>();
	}

	private Rect CalculateBounds()
	{
		Vector3 position = base.transform.position;
		if (position == m_PrevPosition && !m_ForceRecalculateBounds)
		{
			return m_CachedBounds;
		}
		m_PrevPosition = position;
		m_ForceRecalculateBounds = false;
		Bounds? bounds = null;
		if (m_Colliders != null)
		{
			for (int i = 0; i < m_Colliders.Length; i++)
			{
				Collider collider = m_Colliders[i];
				if (collider.enabled && collider.gameObject.activeInHierarchy)
				{
					if (!bounds.HasValue)
					{
						bounds = collider.bounds;
					}
					else
					{
						bounds.Value.Encapsulate(collider.bounds);
					}
				}
			}
		}
		Bounds valueOrDefault = bounds.GetValueOrDefault();
		if (!bounds.HasValue)
		{
			valueOrDefault = new Bounds(base.ViewTransform.position, Vector3.one);
			bounds = valueOrDefault;
		}
		m_CachedBounds = new Rect
		{
			xMin = bounds.Value.min.x,
			yMin = bounds.Value.min.z,
			xMax = bounds.Value.max.x,
			yMax = bounds.Value.max.z
		};
		return m_CachedBounds;
	}

	public void ChangeStage(DestructionStage stage, bool onLoad)
	{
		m_ForceRecalculateBounds = true;
		m_DestructionStage = stage;
		UpdateHighlight();
		DestructionStageSettings destructionStageSettings = m_DestructionStages.FirstItem((DestructionStageSettings i) => i.Type == stage);
		GridNavmeshModifier gridNavmeshModifier = destructionStageSettings?.NavmeshModifier;
		if (destructionStageSettings != null && gridNavmeshModifier != null && gridNavmeshModifier != GridNavmeshModifier)
		{
			if (GridNavmeshModifier != null)
			{
				GridNavmeshModifier.gameObject.SetActive(value: false);
			}
			if (gridNavmeshModifier != null)
			{
				gridNavmeshModifier.gameObject.SetActive(value: true);
			}
			GridNavmeshModifier = gridNavmeshModifier;
		}
		EventBus.RaiseEvent((IMapObjectEntity)Data, (Action<IDestructibleEntityHandler>)delegate(IDestructibleEntityHandler h)
		{
			h.HandleDestructionStageChanged(stage);
		}, isCheckRuntime: true);
	}

	protected override void OnDrawGizmosSelected()
	{
		if (m_Colliders.Empty())
		{
			m_Colliders = GetComponentsInChildren<Collider>(includeInactive: true);
		}
		base.OnDrawGizmosSelected();
	}

	protected override bool ShouldBeHighlighted()
	{
		if (CanBeAttackedDirectly && m_DestructionStage != DestructionStage.Destroyed)
		{
			return base.ShouldBeHighlighted();
		}
		return false;
	}

	public override void UpdateViewActive()
	{
		base.UpdateViewActive();
		if (Data.IsViewActive)
		{
			Data.GetDestructionStagesManagerOptional()?.UpdateOnIsInGameTrue();
		}
	}

	string IDestructionStagesManager.get_name()
	{
		return base.name;
	}
}
