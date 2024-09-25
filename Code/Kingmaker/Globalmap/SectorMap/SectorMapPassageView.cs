using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

[RequireComponent(typeof(LineRenderer))]
[KnowledgeDatabaseID("734b66bd11a64e7bb6c2469e683bf849")]
public class SectorMapPassageView : EntityViewBase
{
	private bool m_VisibilityFromFilter = true;

	[SerializeField]
	public SectorMapObject StarSystem1;

	[SerializeField]
	public SectorMapObject StarSystem2;

	[SerializeField]
	public SectorMapPassageEntity.PassageDifficulty Difficulty;

	[SerializeField]
	public bool ShouldBeExploredFromBothSystems;

	public bool IsExploredOnStart;

	public SectorMapObjectEntity StarSystem1Entity
	{
		get
		{
			if (!(StarSystem1 != null))
			{
				return Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity obj) => obj.Blueprint == Data?.StarSystem1Blueprint);
			}
			return StarSystem1.Data;
		}
	}

	public SectorMapObjectEntity StarSystem2Entity
	{
		get
		{
			if (!(StarSystem2 != null))
			{
				return Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity obj) => obj.Blueprint == Data?.StarSystem2Blueprint);
			}
			return StarSystem2.Data;
		}
	}

	public new SectorMapPassageEntity Data => (SectorMapPassageEntity)base.Data;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new SectorMapPassageEntity(this));
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
		SetVisible(m_VisibilityFromFilter && Data.IsExplored);
	}

	public void ChangeVisual(SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		LineRenderer component = base.gameObject.GetComponent<LineRenderer>();
		BlueprintWarpRoutesSettings.DifficultySettings difficultySettings = BlueprintWarhammerRoot.Instance.WarpRoutesSettings.DifficultySettingsList.FirstOrDefault((BlueprintWarpRoutesSettings.DifficultySettings setting) => setting.Difficulty == difficulty);
		if (!(component == null) && difficultySettings != null)
		{
			component.colorGradient = difficultySettings.Prefab.Load()?.GetComponent<LineRenderer>()?.colorGradient;
			component.material = difficultySettings.WarpRouteMaterial;
			component.textureScale = difficultySettings.WarpRouteTextureScale;
			component.widthMultiplier = 0f;
			component.widthMultiplier = difficultySettings.WarpRouteWidth;
		}
	}

	public void LoadPassageVisualPoints()
	{
		CurvedLineRenderer component = GetComponent<CurvedLineRenderer>();
		CurvedLinePoint[] componentsInChildren = GetComponentsInChildren<CurvedLinePoint>();
		if (componentsInChildren.Length != 0)
		{
			CurvedLinePoint[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				Object.DestroyImmediate(array[i].gameObject);
			}
		}
		base.ViewTransform.rotation = Quaternion.AngleAxis(90f, new Vector3(1f, 0f, 0f));
		foreach (Vector3 curvedLinePoint in Data.CurvedLinePoints)
		{
			GameObject obj = new GameObject();
			obj.name = base.name.Replace("(Clone)", "") + "_BezierPoint";
			obj.AddComponent<CurvedLinePoint>();
			obj.transform.position = curvedLinePoint;
			obj.transform.parent = base.ViewTransform;
		}
		ChangeVisual(Data.CurrentDifficulty);
		LineRenderer component2 = base.gameObject.GetComponent<LineRenderer>();
		component.lineWidth = ((component2 != null) ? component2.widthMultiplier : 1f);
		component.ManualUpdate();
	}

	private void OnValidate()
	{
		base.gameObject.name = "PS: " + StarSystem1?.name + " <--> " + StarSystem2?.name;
	}

	public void ChangeAlpha(float alpha)
	{
		LineRenderer component = GetComponent<LineRenderer>();
		component.startColor = new Color(component.startColor.r, component.startColor.g, component.startColor.b, alpha);
		component.endColor = new Color(component.endColor.r, component.endColor.g, component.endColor.b, alpha);
	}
}
