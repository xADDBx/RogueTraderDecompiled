using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

public static class HighlightConstantBuffer
{
	public static int _ZTest = Shader.PropertyToID("_ZTest");

	public static int _ZWrite = Shader.PropertyToID("_ZWrite");

	public static int _Color = Shader.PropertyToID("_Color");

	public static int _Alphatest = Shader.PropertyToID("_Alphatest");

	public static int _CullMode = Shader.PropertyToID("_CullMode");

	public static int _BaseMap = Shader.PropertyToID("_BaseMap");

	public static int _Cutoff = Shader.PropertyToID("_Cutoff");

	public static int _VatType = Shader.PropertyToID("_VatType");

	public static int _PosVatMap = Shader.PropertyToID("_PosVatMap");

	public static int _RotVatMap = Shader.PropertyToID("_RotVatMap");

	public static int _VatNumOfFrames = Shader.PropertyToID("_VatNumOfFrames");

	public static int _VatPosMin = Shader.PropertyToID("_VatPosMin");

	public static int _VatPosMax = Shader.PropertyToID("_VatPosMax");

	public static int _VatPivMin = Shader.PropertyToID("_VatPivMin");

	public static int _VatPivMax = Shader.PropertyToID("_VatPivMax");

	public static int _VatLerp = Shader.PropertyToID("_VatLerp");

	public static int _VatCurrentFrame = Shader.PropertyToID("_VatCurrentFrame");

	public static int _HighlightingBlurOffset = Shader.PropertyToID("_HighlightingBlurOffset");
}
