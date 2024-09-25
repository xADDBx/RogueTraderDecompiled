using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("64612aed396d34c4282d7ac82651370e")]
public class MapObjectRevealed : Condition
{
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public override string GetDescription()
	{
		return "Заметил ли игрок мапобжект. Если мапобжект не под персепшеном, то он считается замеченным когда игрок его увидел в тумане войны";
	}

	protected override string GetConditionCaption()
	{
		return $"MapObject {MapObject} revealed";
	}

	protected override bool CheckCondition()
	{
		return MapObject.GetValue().IsRevealed;
	}
}
