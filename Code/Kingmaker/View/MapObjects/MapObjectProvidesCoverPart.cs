using System.Linq;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Covers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class MapObjectProvidesCoverPart : ViewBasedPart<MapObjectForcedCoverSettings>, IDestructibleEntityHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IDynamicCoverProvider, IHashable
{
	[HideInInspector]
	private DestructionStage m_CurrentDestructionStage;

	public NodeList Nodes => ((MapObjectEntity)base.Owner).GetOccupiedNodes();

	public LosCalculations.CoverType CoverType => base.Settings.CoverType;

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		Game.Instance.ForcedCoversController.RegisterCoverProvider(this);
		PartDestructionStagesManager optional = base.Owner.GetOptional<PartDestructionStagesManager>();
		if (optional != null)
		{
			m_CurrentDestructionStage = optional.Stage;
		}
	}

	protected override void OnDetach()
	{
		Game.Instance.ForcedCoversController.UnregisterCoverProvider(this);
		base.OnDetach();
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		base.OnSettingsDidSet(isNewSettings);
		base.Settings.StageToCoverTypeMap = base.Settings.DestructionStageToCovers.ToDictionary((MapObjectForcedCoverSettings.DestructionStageToCover x) => x.DestructionStage, (MapObjectForcedCoverSettings.DestructionStageToCover x) => x.CoverType);
		UpdateCoverType();
	}

	public void HandleDestructionStageChanged(DestructionStage stage)
	{
		if (EventInvokerExtensions.MapObjectEntity == base.Owner)
		{
			m_CurrentDestructionStage = stage;
			UpdateCoverType();
		}
	}

	private void UpdateCoverType()
	{
		base.Settings.CoverType = (base.Settings.StageToCoverTypeMap.TryGetValue(m_CurrentDestructionStage, out var value) ? value : LosCalculations.CoverType.None);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
