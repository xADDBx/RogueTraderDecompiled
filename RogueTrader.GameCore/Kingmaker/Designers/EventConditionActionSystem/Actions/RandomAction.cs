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
		int maxExclusive = array.Aggregate(0, (int w, ActionAndWeight a) => w + a.Weight);
		int num = PFStatefulRandom.Action.Range(0, maxExclusive);
		int num2 = 0;
		ActionAndWeight[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ActionAndWeight actionAndWeight = array2[i];
			ActionList action = actionAndWeight.Action;
			int num3 = num2 + actionAndWeight.Weight;
			if (num2 <= num && num < num3)
			{
				action.Run();
				break;
			}
			num2 = num3;
		}
	}
}
