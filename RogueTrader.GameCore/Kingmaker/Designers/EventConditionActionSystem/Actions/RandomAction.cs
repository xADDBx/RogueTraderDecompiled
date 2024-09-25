using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("fe04f935f78d4ba4c805faf9a4be38a3")]
public class RandomAction : GameAction
{
	public ActionAndWeight[] Actions;

	public override string GetDescription()
	{
		return $"Выполняет случайный набор экшенов с учетом веса и условий ({Actions.EmptyIfNull().Count()} выриантов)";
	}

	public override string GetCaption()
	{
		return $"Random action ({Actions.EmptyIfNull().Count()} variants)";
	}

	protected override void RunAction()
	{
		ActionAndWeight[] array = (from a in Actions.EmptyIfNull()
			where a.Conditions.Check()
			select a).ToArray();
		int num = array.Aggregate(0, (int w, ActionAndWeight a) => w + a.Weight);
		if (num == 0)
		{
			PFLog.Actions.Error("Total sum of RandomActions " + base.AssetGuid + " is zero!");
			int num2 = PFStatefulRandom.Action.Range(0, array.Length);
			array[num2].Action.Run();
			return;
		}
		int num3 = PFStatefulRandom.Action.Range(0, num);
		int num4 = 0;
		ActionAndWeight[] array2 = array;
		foreach (ActionAndWeight actionAndWeight in array2)
		{
			ActionList action = actionAndWeight.Action;
			int num5 = num4 + actionAndWeight.Weight;
			if (num4 <= num3 && num3 < num5)
			{
				action.Run();
				break;
			}
			num4 = num5;
		}
	}
}
