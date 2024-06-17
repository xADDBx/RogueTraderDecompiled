using System.Collections.Generic;
using Kingmaker.Blueprints.Base;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components;

public class TextureSequentialSelectorVM : SequentialSelectorVM<TextureSequentialEntity>
{
	private readonly ReactiveProperty<Sprite> m_Value = new ReactiveProperty<Sprite>(null);

	public IReadOnlyReactiveProperty<Sprite> Value => m_Value;

	public TextureSequentialSelectorVM(bool cyclical = true)
		: base(cyclical)
	{
	}

	public TextureSequentialSelectorVM(List<TextureSequentialEntity> valueList, TextureSequentialEntity current = null, bool cyclical = true)
		: base(valueList, current, cyclical)
	{
	}

	protected override void SetCurrentEntity()
	{
		m_Value.Value = ValueList[base.CurrentIndex.Value].Texture;
	}

	protected override void SetSelectUIGender(Gender gender, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.Gender && !UINetUtility.IsControlMainCharacter())
		{
			m_Value.Value = ValueList[index].Texture;
		}
	}
}
