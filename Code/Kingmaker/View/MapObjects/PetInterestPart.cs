using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class PetInterestPart : ViewBasedPart<PetInterestSettings>, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	public const string PET_PARAM_NAME = "Pet";

	public const string POSITION_PARAM_NAME = "Point";

	private double m_StartTime;

	private CutscenePlayerView m_CutscenePlayerView;

	private UnitReference m_InterestedOwner;

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (base.Settings.PetCutscenes.Length == 0 || Game.Instance.TurnController.InCombat || Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			return;
		}
		if (m_CutscenePlayerView != null)
		{
			if (m_InterestedOwner != unit)
			{
				return;
			}
			UnitPartPetOwner optional = unit.GetOptional<UnitPartPetOwner>();
			if (optional != null)
			{
				optional.LastReactionMoment = Game.Instance.TimeController.GameTime.TotalMilliseconds;
			}
			CutscenePlayerData playerData = m_CutscenePlayerView.PlayerData;
			if (playerData == null || playerData.IsFinished)
			{
				m_StartTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
				m_CutscenePlayerView = null;
				return;
			}
			if (unit.DistanceTo(base.Owner.View.ViewTransform.position) >= base.Settings.Radius)
			{
				m_CutscenePlayerView.PlayerData.Stop();
				m_StartTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
				m_CutscenePlayerView = null;
				return;
			}
		}
		if (!base.Owner.IsInGame || Game.Instance.TimeController.GameTime.TotalMilliseconds - m_StartTime < (double)(base.Settings.ReactionCooldown * 1000) || !unit.IsInPlayerParty || !Game.Instance.SelectionCharacter.SelectedUnits.Contains(unit as BaseUnitEntity))
		{
			return;
		}
		UnitPartPetOwner petOwner = unit.GetOptional<UnitPartPetOwner>();
		if (petOwner == null || Game.Instance.TimeController.GameTime.TotalMilliseconds - petOwner.LastReactionMoment < 3000.0 || base.Settings.PetCutscenes.All((CutsceneByPet p) => p.PetyType != petOwner.PetType) || !(unit.DistanceTo(base.Owner.View.ViewTransform.position) < base.Settings.Radius))
		{
			return;
		}
		uint num = base.Owner.View.ViewTransform.position.GetNearestNodeXZ()?.Area ?? 0;
		uint num2 = petOwner.PetUnit.MovementAgent.Position.GetNearestNodeXZ()?.Area ?? 0;
		if (num != num2)
		{
			return;
		}
		m_StartTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
		PathfindingService.Instance.FindPathRT_Delayed(petOwner.PetUnit.MovementAgent, base.Owner.View.ViewTransform.position, 0f, 1, delegate(ForcedPath path)
		{
			if (!path.error && !(path.GetTotalLength() > base.Settings.PetPathLimit))
			{
				petOwner.LastReactionMoment = Game.Instance.TimeController.GameTime.TotalMilliseconds;
				m_CutscenePlayerView = CutscenePlayerView.Play(base.Settings.PetCutscenes.First((CutsceneByPet p) => p.PetyType == petOwner.PetType).Cutscene.Get(), new ParametrizedContextSetter
				{
					AdditionalParams = 
					{
						{
							"Pet",
							(object)petOwner.PetUnit.FromAbstractUnitEntity()
						},
						{
							"Point",
							(object)base.Owner.View.ViewTransform.position
						}
					}
				}, queued: false, unit.HoldingState);
				m_InterestedOwner = UnitReference.FromIAbstractUnitEntity(unit);
			}
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
