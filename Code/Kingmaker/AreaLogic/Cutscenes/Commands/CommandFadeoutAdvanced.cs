using System;
using DG.Tweening;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Fade;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("31c02f18dee94abcafc847144d26059f")]
public class CommandFadeoutAdvanced : CommandFadeoutBase
{
	public class FadeEaseOrderAttribute : EnumOrderAttribute
	{
		private static readonly Enum[] s_order = new Enum[19]
		{
			Ease.Linear,
			Ease.InSine,
			Ease.OutSine,
			Ease.InOutSine,
			Ease.InQuad,
			Ease.OutQuad,
			Ease.InOutQuad,
			Ease.InCubic,
			Ease.OutCubic,
			Ease.InOutCubic,
			Ease.InQuart,
			Ease.OutQuart,
			Ease.InOutQuart,
			Ease.InQuint,
			Ease.OutQuint,
			Ease.InOutQuint,
			Ease.InExpo,
			Ease.OutExpo,
			Ease.InOutExpo
		};

		public override Enum[] Order => s_order;
	}

	[SerializeField]
	private float m_FadeoutDuration = 0.3f;

	[FadeEaseOrder]
	[SerializeField]
	[Tooltip("DOTween Ease function (See confluence page 'CommandFadeoutAdvanced' for help)")]
	private Ease m_FadeoutEase = Ease.Linear;

	[SerializeField]
	private float m_PauseDuration = 0.4f;

	[SerializeField]
	private float m_FadeinDuration = 0.3f;

	[FadeEaseOrder]
	[SerializeField]
	[Tooltip("DOTween Ease function (See confluence page 'CommandFadeoutAdvanced' for help)")]
	private Ease m_FadeinEase = Ease.Linear;

	protected override float Lifetime => m_FadeoutDuration + m_PauseDuration;

	protected override void Fadeout(bool fade)
	{
		FadeCanvas.Fadeout(fade, fade ? m_FadeoutDuration : m_FadeinDuration, fade ? m_FadeoutEase : m_FadeinEase);
	}

	public override string GetCaption()
	{
		return "Fade screen advanced";
	}
}
