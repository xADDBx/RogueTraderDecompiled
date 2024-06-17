using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class PlanetView : StarSystemObjectView
{
	public new PlanetEntity Data => base.Data as PlanetEntity;

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		RefreshVisualSets();
	}

	protected override Entity CreateEntityDataImpl(bool load)
	{
		return Entity.Initialize(new PlanetEntity(this, (BlueprintPlanet)(BlueprintStarSystemObject)Blueprint));
	}

	private void RefreshVisualSets()
	{
		if (!Game.Instance.Player.StarSystemsState.PlanetChangedVisualPrefabs.TryGetValue(Data.Blueprint, out var value) || value == null)
		{
			return;
		}
		foreach (Transform item in BaseVisualRoot.transform.Children())
		{
			item.gameObject.SetActive(value: false);
		}
		GameObject gameObject = Object.Instantiate(value.PrefabLink.Load(), BaseVisualRoot.transform.position, BaseVisualRoot.transform.rotation, BaseVisualRoot.transform);
		gameObject.SetActive(value: true);
		BaseVisualRoot.SetActive(value: true);
		base.VisualRoot = gameObject;
	}

	public void SetNewVisual()
	{
		RefreshVisualSets();
	}
}
