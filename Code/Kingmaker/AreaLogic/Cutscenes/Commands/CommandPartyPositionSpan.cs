using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("7c4c2d1e55c01954485a778e4d8ecb19")]
public class CommandPartyPositionSpan : CommandBase
{
	private class PartyMemberEntry
	{
		public BlueprintUnit Blueprint;

		public Vector3 Position;

		public Vector3[] PetsPositions;

		public float Orientation;

		public float[] PetsOrientations;
	}

	private class Data
	{
		public readonly List<PartyMemberEntry> Entries = new List<PartyMemberEntry>();
	}

	public bool OnlyIfCanMove;

	[InfoBox(Text = "В условии юнит, которого хотим вернуть, доступен через PartyUnit")]
	public ConditionsChecker ReturnCondition;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Entries.Clear();
		foreach (UnitReference partyCharacter in Game.Instance.Player.PartyCharacters)
		{
			if (partyCharacter.Entity != null)
			{
				PartyMemberEntry item = new PartyMemberEntry
				{
					Blueprint = partyCharacter.Entity.ToBaseUnitEntity().Blueprint,
					Position = partyCharacter.Entity.Position,
					Orientation = partyCharacter.Entity.ToBaseUnitEntity().Orientation
				};
				commandData.Entries.Add(item);
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		base.OnStop(player);
		foreach (PartyMemberEntry entry in player.GetCommandData<Data>(this).Entries)
		{
			BaseUnitEntity baseUnitEntity = Game.Instance.Player.PartyCharacters.FirstOrDefault((UnitReference c) => c.Entity?.ToBaseUnitEntity().Blueprint == entry.Blueprint).Entity.ToBaseUnitEntity();
			if (baseUnitEntity != null && ShouldReturn(baseUnitEntity))
			{
				baseUnitEntity.Position = entry.Position;
				baseUnitEntity.SetOrientation(entry.Orientation);
			}
		}
	}

	private bool ShouldReturn(BaseUnitEntity character)
	{
		if (OnlyIfCanMove && !character.State.CanMove)
		{
			return false;
		}
		if (ReturnCondition.HasConditions)
		{
			using (ContextData<PartyUnitData>.Request().Setup(character))
			{
				if (!ReturnCondition.Check())
				{
					return false;
				}
			}
		}
		return true;
	}

	public override string GetCaption()
	{
		return "<b>Store</b> party positions";
	}
}
