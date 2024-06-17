using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.QA;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.BaseGetter;

public class PropertyGetterException : OwlcatException
{
	public readonly PropertyGetter Getter;

	private string m_Message;

	public override string Message => m_Message ?? (m_Message = GetMessage());

	public PropertyGetterException([NotNull] PropertyGetter getter, [CanBeNull] Exception innerException)
		: base(innerException)
	{
		Getter = getter;
	}

	private string GetMessage()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("Exception in Property Getter: ");
		if (Getter == null)
		{
			builder.Append(base.InnerException?.Message ?? "unknown");
			return builder.ToString();
		}
		SimpleBlueprint owner = Getter.Owner;
		if (owner != null)
		{
			builder.Append(owner.name);
			builder.Append('.');
		}
		builder.Append(Getter.GetType().Name);
		builder.Append("#");
		builder.Append(Getter.AssetGuidShort);
		if (base.InnerException != null)
		{
			builder.Append('[');
			builder.Append(base.InnerException.Message);
			builder.Append(']');
		}
		return builder.ToString();
	}

	protected override void ReportInternal(LogChannel channel, UnityEngine.Object context, bool showReportWindow)
	{
		Owlcat.Runtime.Core.Logging.Logger.Instance.Log(channel, context, LogSeverity.Error, base.InnerException ?? this, Message, null);
		QAModeExceptionReporter.MaybeShowError(Message);
	}
}
