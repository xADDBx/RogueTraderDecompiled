using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[TypeId("15e731eb83be4db9a1840339fff7b8ea")]
public class EtudeBracketShowCounterUI : EtudeBracketTrigger, IHashable
{
	public LocalizedString Label;

	public bool ShowSubLabel;

	[ConditionalShow("ShowSubLabel")]
	public LocalizedString SubLabel;

	public PropertyCalculator Value;

	public PropertyCalculator TargetValue;

	public EtudeUICounterTypes UICounterType = EtudeUICounterTypes.Label;

	protected override void OnEnter()
	{
		Show();
	}

	protected override void OnExit()
	{
		Hide();
	}

	protected override void OnResume()
	{
		Show();
	}

	private string GetCounterId()
	{
		return base.Fact.Blueprint.AssetGuid + name;
	}

	private void Show()
	{
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		PropertyContext context = new PropertyContext(mainCharacterEntity, null, null, base.Context);
		EventBus.RaiseEvent(delegate(IEtudeCounterHandler h)
		{
			h.ShowEtudeCounter(new EtudeShowCounterUIStruct
			{
				Id = GetCounterId(),
				Type = UICounterType,
				Label = Label,
				ShowSubLabel = ShowSubLabel,
				SubLabel = SubLabel,
				ValueGetter = () => Value.GetValue(context),
				TargetValueGetter = () => TargetValue.GetValue(context)
			});
		});
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IEtudeCounterHandler h)
		{
			h.HideEtudeCounter(GetCounterId());
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
