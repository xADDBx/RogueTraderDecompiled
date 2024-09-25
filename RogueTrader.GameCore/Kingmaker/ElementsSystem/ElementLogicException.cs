using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.QA;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

public class ElementLogicException : OwlcatException
{
	private static readonly StringBuilder StringBuilder = new StringBuilder();

	public readonly Element Element;

	private string m_Message;

	public override string Message => m_Message ?? (m_Message = GetMessage());

	public ElementLogicException([NotNull] Element element, Exception innerException = null)
		: base(innerException)
	{
		Element = element;
		ReportingUtilsSourceHolder.Instance.ExceptionSource = Element.Owner as BlueprintScriptableObject;
	}

	public virtual string GetPrefixText()
	{
		return "Exception in game logic";
	}

	private string GetMessage()
	{
		StringBuilder stringBuilder = StringBuilder;
		stringBuilder.Clear();
		stringBuilder.Append(GetPrefixText());
		stringBuilder.Append(": ");
		ICutscene cutscene = CutscenePlayerDataScope.Current?.ICutscene;
		if (cutscene != null)
		{
			stringBuilder.Append(cutscene.Name);
			stringBuilder.Append('.');
		}
		SimpleBlueprint owner = Element.Owner;
		if (owner != null && owner != CutscenePlayerDataScope.Current?.ICutscene)
		{
			stringBuilder.Append(owner.name);
			stringBuilder.Append(".");
		}
		stringBuilder.Append(Element.GetType().Name);
		stringBuilder.Append("#");
		stringBuilder.Append(Element.AssetGuidShort);
		if (base.InnerException != null)
		{
			stringBuilder.Append(" [");
			stringBuilder.Append(base.InnerException.Message);
			stringBuilder.Append("]");
		}
		string value = owner?.AssetGuid ?? string.Empty;
		stringBuilder.Append(" (");
		stringBuilder.Append(value);
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	protected sealed override void ReportInternal(LogChannel channel, UnityEngine.Object context, bool showReportWindow)
	{
		Owlcat.Runtime.Core.Logging.Logger.Instance.Log(channel, context, LogSeverity.Error, base.InnerException ?? this, Message, null);
		QAModeExceptionEvents.Instance.MaybeShowError(Message);
	}
}
