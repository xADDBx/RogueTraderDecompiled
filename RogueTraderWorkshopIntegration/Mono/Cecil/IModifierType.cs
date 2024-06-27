using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IModifierType
{
	TypeReference ModifierType { get; }

	TypeReference ElementType { get; }
}
