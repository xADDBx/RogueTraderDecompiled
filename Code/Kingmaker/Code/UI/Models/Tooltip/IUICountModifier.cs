using JetBrains.Annotations;

namespace Kingmaker.Code.UI.Models.Tooltip;

public interface IUICountModifier
{
	[CanBeNull]
	string CountText { get; }
}
