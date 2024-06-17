using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("28f3698ba62041e09a5abcbe59a14725")]
public class MakeAutoSave : GameAction
{
	public bool SaveForImport;

	public override string GetDescription()
	{
		return "Вызывает принудительное автосохранение.\n" + $"SaveForImport({SaveForImport}) - сделать ли специальный секретный сейв как в DLC2 PF:K";
	}

	public override string GetCaption()
	{
		return "Make autosave" + (SaveForImport ? " for later import" : "");
	}

	public override void RunAction()
	{
		if (SaveForImport)
		{
			Game.Instance.SaveGame(Game.Instance.SaveManager.GetSpecialImportSlot());
		}
		else
		{
			Game.Instance.SaveGame(Game.Instance.SaveManager.GetNextAutoslot());
		}
	}
}
