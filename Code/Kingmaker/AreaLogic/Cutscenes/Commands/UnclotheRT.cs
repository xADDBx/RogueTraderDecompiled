using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[TypeId("e123a430872f4f13b53b94326e2725ba")]
public class UnclotheRT : CommandBase
{
	public bool ClotheON;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		if (playerCharacter?.View == null || playerCharacter.View.CharacterAvatar == null)
		{
			return;
		}
		if (!ClotheON)
		{
			playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneEquipment.Clear();
			playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneRampIndices.Clear();
			foreach (EquipmentEntity equipmentEntity2 in playerCharacter.View.CharacterAvatar.EquipmentEntities)
			{
				playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneEquipment.Add(equipmentEntity2);
			}
			foreach (Character.SelectedRampIndices rampIndex in playerCharacter.View.CharacterAvatar.RampIndices)
			{
				playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneRampIndices.Add(rampIndex);
			}
			List<EquipmentEntityLink> source = ((playerCharacter.Gender == Gender.Male) ? BlueprintRoot.Instance.CharGenRoot.MaleDontUnequip : BlueprintRoot.Instance.CharGenRoot.FemaleDontUnequip);
			playerCharacter.View.CharacterAvatar.RemoveAllEquipmentEntities();
			IEnumerable<EquipmentEntity> source2 = source.Select((EquipmentEntityLink x) => x.Load());
			foreach (EquipmentEntity equipmentEntity in playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneEquipment)
			{
				if (source2.Contains(equipmentEntity))
				{
					playerCharacter.View.CharacterAvatar.AddEquipmentEntity(equipmentEntity);
				}
				Character.SelectedRampIndices selectedRampIndices = playerCharacter.View.CharacterAvatar.SavedBeforeCutsceneRampIndices.FirstOrDefault((Character.SelectedRampIndices x) => x.EquipmentEntity == equipmentEntity);
				if (selectedRampIndices != null)
				{
					playerCharacter.View.CharacterAvatar.SetRampIndices(equipmentEntity, selectedRampIndices.PrimaryIndex, selectedRampIndices.SecondaryIndex);
				}
			}
			EquipmentEntityLink equipmentEntityLink = ((playerCharacter.Gender == Gender.Male) ? BlueprintRoot.Instance.CharGenRoot.MaleClothes[0] : BlueprintRoot.Instance.CharGenRoot.FemaleClothes[0]);
			if (equipmentEntityLink != null)
			{
				playerCharacter.View.CharacterAvatar.AddEquipmentEntity(equipmentEntityLink);
			}
			playerCharacter.View.HandsEquipment.UpdateVisibility(isVisible: false);
			playerCharacter.View.HandsEquipment.HiddenByCutscene = true;
		}
		else
		{
			playerCharacter.View.CharacterAvatar.RestoreEquipment();
			playerCharacter.View.HandsEquipment.HiddenByCutscene = false;
			playerCharacter.View.HandsEquipment.UpdateVisibility(isVisible: true);
		}
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		playerCharacter.View.CharacterAvatar.RestoreEquipment();
		playerCharacter.View.HandsEquipment.UpdateVisibility(isVisible: true);
	}
}
