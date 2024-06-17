using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Visual;

public static class MaterialPropertyBlockExtensions
{
	public static readonly int _Color = Shader.PropertyToID("_Color");

	public static void SetBaseColor(this MaterialPropertyBlock matProp, Color color)
	{
		matProp.SetColor(_Color, color);
	}

	public static Tweener DOFade(this MaterialPropertyBlock matProp, float endValue, float duration)
	{
		return DOTween.ToAlpha(() => matProp.GetVector(_Color), delegate(Color x)
		{
			matProp.SetColor(_Color, x);
		}, endValue, duration).SetTarget(matProp);
	}
}
