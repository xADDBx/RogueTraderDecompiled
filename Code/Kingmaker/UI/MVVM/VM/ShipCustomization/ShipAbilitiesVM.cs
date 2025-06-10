using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization;

public class ShipAbilitiesVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<CharInfoFeatureVM> ActiveAbilities = new List<CharInfoFeatureVM>();

	public readonly List<CharInfoFeatureVM> PassiveAbilities = new List<CharInfoFeatureVM>();

	public ShipAbilitiesVM()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		Ability[] array = UIUtilityUnit.CollectAbilities(playerShip).ToArray();
		IEnumerable<UIFeature> enumerable = UIUtilityUnit.CollectFeats(playerShip);
		Ability[] array2 = array;
		foreach (Ability ability in array2)
		{
			if (!ability.Hidden && !ability.Blueprint.IsCantrip)
			{
				CharInfoFeatureVM item = new CharInfoFeatureVM(ability, playerShip);
				ActiveAbilities.Add(item);
			}
		}
		foreach (UIFeature item3 in enumerable)
		{
			BlueprintFeature feature = item3.Feature;
			if (!string.IsNullOrEmpty(feature.Name) && !feature.HideInUI)
			{
				CharInfoFeatureVM item2 = new CharInfoFeatureVM(item3, playerShip);
				PassiveAbilities.Add(item2);
			}
		}
	}

	protected override void DisposeImplementation()
	{
		ActiveAbilities.ForEach(delegate(CharInfoFeatureVM slotVm)
		{
			slotVm.Dispose();
		});
		ActiveAbilities.Clear();
		PassiveAbilities.ForEach(delegate(CharInfoFeatureVM slotVm)
		{
			slotVm.Dispose();
		});
		PassiveAbilities.Clear();
	}
}
