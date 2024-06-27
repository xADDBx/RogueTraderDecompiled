using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class LineNumberTable
{
	protected LineNumberEntry[] _line_numbers;

	public readonly int LineBase;

	public readonly int LineRange;

	public readonly byte OpcodeBase;

	public readonly int MaxAddressIncrement;

	public const int Default_LineBase = -1;

	public const int Default_LineRange = 8;

	public const byte Default_OpcodeBase = 9;

	public const byte DW_LNS_copy = 1;

	public const byte DW_LNS_advance_pc = 2;

	public const byte DW_LNS_advance_line = 3;

	public const byte DW_LNS_set_file = 4;

	public const byte DW_LNS_const_add_pc = 8;

	public const byte DW_LNE_end_sequence = 1;

	public const byte DW_LNE_MONO_negate_is_hidden = 64;

	internal const byte DW_LNE_MONO__extensions_start = 64;

	internal const byte DW_LNE_MONO__extensions_end = 127;

	public LineNumberEntry[] LineNumbers => _line_numbers;

	protected LineNumberTable(MonoSymbolFile file)
	{
		LineBase = file.OffsetTable.LineNumberTable_LineBase;
		LineRange = file.OffsetTable.LineNumberTable_LineRange;
		OpcodeBase = (byte)file.OffsetTable.LineNumberTable_OpcodeBase;
		MaxAddressIncrement = (255 - OpcodeBase) / LineRange;
	}

	internal LineNumberTable(MonoSymbolFile file, LineNumberEntry[] lines)
		: this(file)
	{
		_line_numbers = lines;
	}

	internal void Write(MonoSymbolFile file, MyBinaryWriter bw, bool hasColumnsInfo, bool hasEndInfo)
	{
		int num = (int)bw.BaseStream.Position;
		bool flag = false;
		int num2 = 1;
		int num3 = 0;
		int num4 = 1;
		for (int i = 0; i < LineNumbers.Length; i++)
		{
			int num5 = LineNumbers[i].Row - num2;
			int num6 = LineNumbers[i].Offset - num3;
			if (LineNumbers[i].File != num4)
			{
				bw.Write((byte)4);
				bw.WriteLeb128(LineNumbers[i].File);
				num4 = LineNumbers[i].File;
			}
			if (LineNumbers[i].IsHidden != flag)
			{
				bw.Write((byte)0);
				bw.Write((byte)1);
				bw.Write((byte)64);
				flag = LineNumbers[i].IsHidden;
			}
			if (num6 >= MaxAddressIncrement)
			{
				if (num6 < 2 * MaxAddressIncrement)
				{
					bw.Write((byte)8);
					num6 -= MaxAddressIncrement;
				}
				else
				{
					bw.Write((byte)2);
					bw.WriteLeb128(num6);
					num6 = 0;
				}
			}
			if (num5 < LineBase || num5 >= LineBase + LineRange)
			{
				bw.Write((byte)3);
				bw.WriteLeb128(num5);
				if (num6 != 0)
				{
					bw.Write((byte)2);
					bw.WriteLeb128(num6);
				}
				bw.Write((byte)1);
			}
			else
			{
				byte value = (byte)(num5 - LineBase + LineRange * num6 + OpcodeBase);
				bw.Write(value);
			}
			num2 = LineNumbers[i].Row;
			num3 = LineNumbers[i].Offset;
		}
		bw.Write((byte)0);
		bw.Write((byte)1);
		bw.Write((byte)1);
		if (hasColumnsInfo)
		{
			for (int j = 0; j < LineNumbers.Length; j++)
			{
				LineNumberEntry lineNumberEntry = LineNumbers[j];
				if (lineNumberEntry.Row >= 0)
				{
					bw.WriteLeb128(lineNumberEntry.Column);
				}
			}
		}
		if (hasEndInfo)
		{
			for (int k = 0; k < LineNumbers.Length; k++)
			{
				LineNumberEntry lineNumberEntry2 = LineNumbers[k];
				if (lineNumberEntry2.EndRow == -1 || lineNumberEntry2.EndColumn == -1 || lineNumberEntry2.Row > lineNumberEntry2.EndRow)
				{
					bw.WriteLeb128(16777215);
					continue;
				}
				bw.WriteLeb128(lineNumberEntry2.EndRow - lineNumberEntry2.Row);
				bw.WriteLeb128(lineNumberEntry2.EndColumn);
			}
		}
		file.ExtendedLineNumberSize += (int)bw.BaseStream.Position - num;
	}

	internal static LineNumberTable Read(MonoSymbolFile file, MyBinaryReader br, bool readColumnsInfo, bool readEndInfo)
	{
		LineNumberTable lineNumberTable = new LineNumberTable(file);
		lineNumberTable.DoRead(file, br, readColumnsInfo, readEndInfo);
		return lineNumberTable;
	}

	private void DoRead(MonoSymbolFile file, MyBinaryReader br, bool includesColumns, bool includesEnds)
	{
		List<LineNumberEntry> list = new List<LineNumberEntry>();
		bool flag = false;
		bool flag2 = false;
		int num = 1;
		int num2 = 0;
		int file2 = 1;
		while (true)
		{
			byte b = br.ReadByte();
			if (b == 0)
			{
				byte b2 = br.ReadByte();
				long position = br.BaseStream.Position + b2;
				b = br.ReadByte();
				switch (b)
				{
				case 1:
				{
					if (flag2)
					{
						list.Add(new LineNumberEntry(file2, num, -1, num2, flag));
					}
					_line_numbers = list.ToArray();
					if (includesColumns)
					{
						for (int i = 0; i < _line_numbers.Length; i++)
						{
							LineNumberEntry lineNumberEntry = _line_numbers[i];
							if (lineNumberEntry.Row >= 0)
							{
								lineNumberEntry.Column = br.ReadLeb128();
							}
						}
					}
					if (!includesEnds)
					{
						return;
					}
					for (int j = 0; j < _line_numbers.Length; j++)
					{
						LineNumberEntry lineNumberEntry2 = _line_numbers[j];
						int num3 = br.ReadLeb128();
						if (num3 == 16777215)
						{
							lineNumberEntry2.EndRow = -1;
							lineNumberEntry2.EndColumn = -1;
						}
						else
						{
							lineNumberEntry2.EndRow = lineNumberEntry2.Row + num3;
							lineNumberEntry2.EndColumn = br.ReadLeb128();
						}
					}
					return;
				}
				case 64:
					flag = !flag;
					flag2 = true;
					break;
				default:
					throw new MonoSymbolFileException("Unknown extended opcode {0:x}", b);
				case 65:
				case 66:
				case 67:
				case 68:
				case 69:
				case 70:
				case 71:
				case 72:
				case 73:
				case 74:
				case 75:
				case 76:
				case 77:
				case 78:
				case 79:
				case 80:
				case 81:
				case 82:
				case 83:
				case 84:
				case 85:
				case 86:
				case 87:
				case 88:
				case 89:
				case 90:
				case 91:
				case 92:
				case 93:
				case 94:
				case 95:
				case 96:
				case 97:
				case 98:
				case 99:
				case 100:
				case 101:
				case 102:
				case 103:
				case 104:
				case 105:
				case 106:
				case 107:
				case 108:
				case 109:
				case 110:
				case 111:
				case 112:
				case 113:
				case 114:
				case 115:
				case 116:
				case 117:
				case 118:
				case 119:
				case 120:
				case 121:
				case 122:
				case 123:
				case 124:
				case 125:
				case 126:
				case 127:
					break;
				}
				br.BaseStream.Position = position;
			}
			else if (b < OpcodeBase)
			{
				switch (b)
				{
				case 1:
					list.Add(new LineNumberEntry(file2, num, -1, num2, flag));
					flag2 = false;
					break;
				case 2:
					num2 += br.ReadLeb128();
					flag2 = true;
					break;
				case 3:
					num += br.ReadLeb128();
					flag2 = true;
					break;
				case 4:
					file2 = br.ReadLeb128();
					flag2 = true;
					break;
				case 8:
					num2 += MaxAddressIncrement;
					flag2 = true;
					break;
				default:
					throw new MonoSymbolFileException("Unknown standard opcode {0:x} in LNT", b);
				}
			}
			else
			{
				b -= OpcodeBase;
				num2 += b / LineRange;
				num += LineBase + b % LineRange;
				list.Add(new LineNumberEntry(file2, num, -1, num2, flag));
				flag2 = false;
			}
		}
	}

	public bool GetMethodBounds(out LineNumberEntry start, out LineNumberEntry end)
	{
		if (_line_numbers.Length > 1)
		{
			start = _line_numbers[0];
			end = _line_numbers[_line_numbers.Length - 1];
			return true;
		}
		start = LineNumberEntry.Null;
		end = LineNumberEntry.Null;
		return false;
	}
}
