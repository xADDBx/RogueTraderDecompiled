using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

[KnowledgeDatabaseID("eb26f7411e934901969622ca676afeca")]
public class StarSystemObjectView : MapObjectView
{
	public enum StarSystemObjectSize
	{
		Small,
		Medium,
		Large
	}

	public BlueprintStarSystemObjectReference Blueprint;

	public StarSystemObjectSize ObjectSize = StarSystemObjectSize.Medium;

	public GameObject BaseVisualRoot;

	public GameObject VisualRoot { get; set; }

	public new StarSystemObjectEntity Data => (StarSystemObjectEntity)base.Data;

	protected override bool HighlightOnHover => ShouldBeHighlightedInternal();

	protected override bool IsClickable => ShouldBeHighlightedInternal();

	public override Entity CreateEntityData(bool load)
	{
		return CreateEntityDataImpl(load);
	}

	protected override void OnDidAttachToData()
	{
		VisualRoot = BaseVisualRoot;
	}

	protected virtual Entity CreateEntityDataImpl(bool load)
	{
		return Entity.Initialize(CreateStarSystemObjectEntityData(load));
	}

	protected StarSystemObjectEntity CreateStarSystemObjectEntityData(bool load)
	{
		BlueprintStarSystemObject blueprintStarSystemObject = Blueprint.Get();
		if (!(blueprintStarSystemObject is BlueprintStar blueprint))
		{
			if (!(blueprintStarSystemObject is BlueprintAsteroid blueprint2))
			{
				if (!(blueprintStarSystemObject is BlueprintCloud blueprint3))
				{
					if (!(blueprintStarSystemObject is BlueprintComet blueprint4))
					{
						if (blueprintStarSystemObject is BlueprintArtificialObject blueprint5)
						{
							return new ArtificialObjectEntity(this, blueprint5);
						}
						return new StarSystemObjectEntity(this, Blueprint.Get());
					}
					return new CometEntity(this, blueprint4);
				}
				return new CloudEntity(this, blueprint3);
			}
			return new AsteroidEntity(this, blueprint2);
		}
		return new StarEntity(this, blueprint);
	}

	protected override bool ShouldBeHighlighted()
	{
		if (base.Highlighted || (Game.Instance.InteractionHighlightController != null && Game.Instance.InteractionHighlightController.IsHighlighting))
		{
			return HighlightOnHover;
		}
		return false;
	}

	private bool ShouldBeHighlightedInternal()
	{
		return Blueprint.Get()?.ShouldBeHighlighted ?? false;
	}

	public virtual bool CheckLanding()
	{
		if (Blueprint.Get() is BlueprintStar)
		{
			return false;
		}
		return Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity obj) => obj is AnomalyEntityData anomalyEntityData && anomalyEntityData.View.BlockedObject == this)?.IsFullyExplored ?? true;
	}
}
