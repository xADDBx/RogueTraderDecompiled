using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/CreaturesAround")]
[AllowMultipleComponents]
[TypeId("64da7ddec4b0d684fb685bc810669a23")]
public class CreaturesAround : GameAction
{
	[SerializeReference]
	public IntEvaluator Radius;

	[SerializeReference]
	public PositionEvaluator Center;

	public bool CheckLos;

	public bool IncludeDead;

	public ActionList Actions;

	public override string GetDescription()
	{
		return "Выполняет экшены на юнитах в радиусе от центра.\nЭкшены выполняются для каждого найденного юнита.\nКаждого юнита можно получить через эвалюатор CreaturesAroundUnit.\n" + $"Центр: {Center}\n" + $"Радиус в метрах: {Radius}\n" + $"Учитывать ли видимость цели: {CheckLos}\n" + $"Работает ли на мертвых юнитах: {IncludeDead}";
	}

	public override string GetCaption()
	{
		return $"Crearures around {Center} (Radius:{Radius})";
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity item in GameHelper.GetTargetsAround(Center.GetValue(), Radius.GetValue(), CheckLos, IncludeDead))
		{
			using (ContextData<CreaturesAroundUnitData>.Request().Setup(item))
			{
				Actions.Run();
			}
		}
	}
}
