using Kingmaker.Blueprints.Base;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Visual.Sound;

namespace Kingmaker.View.Spawners;

public class SpawningData : ContextData<SpawningData>
{
	public string PrefabGuid { get; private set; }

	public BlueprintRace Race { get; private set; }

	public Gender Gender { get; private set; }

	public BlueprintUnitAsksList Voice { get; private set; }

	public SpawningData Setup(string prefabGuid, BlueprintRace race, Gender gender, BlueprintUnitAsksList voice)
	{
		PrefabGuid = prefabGuid;
		Race = race;
		Gender = gender;
		Voice = voice;
		return this;
	}

	protected override void Reset()
	{
		PrefabGuid = null;
		Race = null;
		Gender = Gender.Male;
		Voice = null;
	}
}
