using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.TextTools.Base;
using UnityEngine;

namespace Kingmaker.TextTools;

public class NarratorStartTemplate : TextTemplate
{
	private class Scope : IDisposable
	{
		public readonly DialogColors Colors;

		public readonly Stack<Scope> Scopes;

		public Scope(DialogColors colors, Stack<Scope> scopes)
		{
			Colors = colors;
			Scopes = scopes;
		}

		public void Dispose()
		{
			if (!Scopes.TryPop(out var result) || result != this)
			{
				PFLog.Default.Error("NarratorStartTemplate.Scope flow is wrong. Please, wrap it into using(NarratorStartTemplate.Scope) { }.");
			}
		}
	}

	private static Stack<Scope> sScopes = new Stack<Scope>();

	public override int Balance => 1;

	public static IDisposable GetScope(DialogColors colors)
	{
		Scope scope = new Scope(colors, sScopes);
		sScopes.Push(scope);
		return scope;
	}

	public override string Generate(bool capitalized, List<string> parameters)
	{
		Scope result;
		DialogColors dialogColors = (sScopes.TryPeek(out result) ? result.Colors : Game.Instance.BlueprintRoot.UIConfig.DialogColors);
		return "<i><color=#" + ColorUtility.ToHtmlStringRGB(dialogColors.Narrator) + ">";
	}
}
