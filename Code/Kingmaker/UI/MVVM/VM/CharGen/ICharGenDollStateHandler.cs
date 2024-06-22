using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Levelup.CharGen;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenDollStateHandler : ISubscriber
{
	void HandleSetGender(Gender gender, int index);

	void HandleSetHead([NotNull] EquipmentEntityLink head, int index);

	void HandleSetRace([NotNull] BlueprintRaceVisualPreset blueprint, int index);

	void HandleSetSkinColor(int index);

	void HandleSetHair([NotNull] EquipmentEntityLink equipmentEntityLink, int index);

	void HandleSetHairColor(int index);

	void HandleSetEyebrows([NotNull] EquipmentEntityLink equipmentEntityLink, int index);

	void HandleSetEyebrowsColor(int index);

	void HandleSetBeard([NotNull] EquipmentEntityLink equipmentEntityLink, int index);

	void HandleSetBeardColor(int index);

	void HandleSetScar([NotNull] EquipmentEntityLink equipmentEntityLink, int index);

	void HandleSetTattoo([NotNull] EquipmentEntityLink equipmentEntityLink, int index, int tattooIndex);

	void HandleSetTattooColor(int rampIndex, int index);

	void HandleSetPort([NotNull] EquipmentEntityLink equipmentEntityLink, int index, int portNumber);

	void HandleSetEquipmentColor(int primaryIndex, int secondaryIndex);

	void HandleShowCloth(bool showCloth);
}
