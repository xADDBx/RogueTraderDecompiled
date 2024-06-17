using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.TextureSelector;

public class TextureSelectorItemView : SelectionGroupEntityView<TextureSelectorItemVM>, IWidgetView
{
	[SerializeField]
	protected Image m_Image;

	[SerializeField]
	protected TextMeshProUGUI m_NumberText;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Texture.Subscribe(SetTexture));
		if (m_NumberText != null)
		{
			TextMeshProUGUI numberText = m_NumberText;
			int number = base.ViewModel.Number;
			numberText.text = number.ToString();
		}
	}

	protected override void OnClick()
	{
		if (UINetUtility.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	private void SetTexture(Texture2D texture)
	{
		m_Image.sprite = ((texture == null) ? null : GetSprite(texture));
	}

	protected virtual Sprite GetSprite(Texture2D texture)
	{
		Rect rect = new Rect(0f, 0f, texture.width, texture.height);
		return Sprite.Create(texture, rect, Vector2.zero);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as TextureSelectorItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is TextureSelectorItemVM;
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		if (value)
		{
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}
		m_Button.CanConfirm = !value;
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
			base.ViewModel.DoFocusMe();
		}
	}
}
