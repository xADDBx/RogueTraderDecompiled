using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Models.LevelUp;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;

public class CharGenPregenSelectorItemVM : SelectionGroupEntityVM
{
	private readonly ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<string> m_CharacterName = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Class = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Role = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Race = new ReactiveProperty<string>();

	private readonly ReactiveProperty<Gender> m_Gender = new ReactiveProperty<Gender>();

	public IReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	public IReadOnlyReactiveProperty<string> CharacterName => m_CharacterName;

	public IReadOnlyReactiveProperty<string> Class => m_Class;

	public IReadOnlyReactiveProperty<string> Role => m_Role;

	public IReadOnlyReactiveProperty<string> Race => m_Race;

	public IReadOnlyReactiveProperty<Gender> Gender => m_Gender;

	public ChargenUnit ChargenUnit { get; private set; }

	public CharGenPregenSelectorItemVM(ChargenUnit chargenUnit, bool isCustomCharacter = false)
		: base(allowSwitchOff: false)
	{
		if (!isCustomCharacter)
		{
			SetupCharacterProperties(chargenUnit);
		}
		else
		{
			SetupCustomCharacterProperties();
		}
	}

	protected override void DisposeImplementation()
	{
		ChargenUnit = null;
	}

	private void SetupCharacterProperties(ChargenUnit chargenUnit)
	{
		ChargenUnit = chargenUnit;
		m_Portrait.Value = ChargenUnit.Blueprint.PortraitSafe.HalfLengthPortrait;
		PregenUnitComponent component = ChargenUnit.Blueprint.GetComponent<PregenUnitComponent>();
		if (component != null)
		{
			m_CharacterName.Value = component.PregenName;
			m_Class.Value = component.PregenClass;
		}
	}

	private void SetupCustomCharacterProperties()
	{
		m_CharacterName.Value = UIStrings.Instance.CharGen.CreateCustomCharacter;
	}

	protected override void DoSelectMe()
	{
	}
}
