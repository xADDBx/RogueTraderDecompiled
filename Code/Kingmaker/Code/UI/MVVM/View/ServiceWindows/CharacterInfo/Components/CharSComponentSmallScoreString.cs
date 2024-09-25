using System;
using Kingmaker.UI.Common.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Components;

public class CharSComponentSmallScoreString : MonoBehaviour
{
	public Image Icon;

	public Color32 IconNormalColor;

	public Color32 IconHighlightColor;

	public Image Background;

	public Color32 BackgroundNormalColor;

	public Color32 BackgroundHighlightColor;

	public TextMeshProUGUI Index;

	public Color32 IndexNormalColor;

	public Color32 IndexHighlightColor;

	public TextMeshProUGUI Label;

	public Color32 LabelNormalColor;

	public Color32 LabelHighlightColor;

	public TextMeshProUGUI Value;

	public Color32 ValueNormalColor;

	public Color32 ValueHighlightColor;

	public FadeAnimator Highlighter;

	public void SetData(string value)
	{
		if (Value.gameObject != null)
		{
			Value.text = value;
		}
	}

	public void SetData(string value, Color32 color)
	{
		if (Value.gameObject != null)
		{
			Value.text = value;
			Value.color = color;
		}
	}

	public void SetLabel(string value)
	{
		if (Label.gameObject != null)
		{
			Label.text = value;
		}
	}

	public void SetIndex(string index)
	{
		if (Index != null)
		{
			Index.text = index;
		}
	}

	public void SetIcon(Sprite image, Color32 color, int? index)
	{
		if (Icon != null)
		{
			if ((bool)image)
			{
				Icon.gameObject.SetActive(value: true);
				Icon.sprite = image;
				Icon.color = color;
			}
			else
			{
				Icon.gameObject.SetActive(value: false);
			}
		}
		if (Index != null)
		{
			Index.text = Convert.ToString(index);
		}
	}

	public void SetBackground(Sprite image, Color32 color)
	{
		if (!(Background == null))
		{
			if ((bool)image)
			{
				Background.gameObject.SetActive(value: true);
				Background.sprite = image;
				Background.color = color;
			}
			else
			{
				Background.gameObject.SetActive(value: false);
			}
		}
	}

	public void Show(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public void Highlight(bool state)
	{
		if (Icon != null)
		{
			Icon.color = (state ? IconHighlightColor : IconNormalColor);
		}
		if (Background != null)
		{
			Background.color = (state ? BackgroundHighlightColor : BackgroundNormalColor);
		}
		if (Label != null)
		{
			Label.color = (state ? LabelHighlightColor : LabelNormalColor);
		}
		if (Index != null)
		{
			Index.color = (state ? IndexHighlightColor : IndexNormalColor);
		}
		if (Value != null)
		{
			Value.color = (state ? ValueHighlightColor : ValueNormalColor);
		}
	}
}
