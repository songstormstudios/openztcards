using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace ZooTycoonCardFlipReimpl;

internal sealed class FontLibrary : IDisposable
{
	private sealed class LoadedFontFace : IDisposable
	{
		public required string SourceName { get; init; }

		public required PrivateFontCollection Collection { get; init; }

		public required FontFamily Family { get; init; }

		public void Dispose()
		{
			Collection.Dispose();
		}
	}

	private readonly List<LoadedFontFace> _faces = new List<LoadedFontFace>();

	private readonly FontFamily _fallbackFamily;

	private readonly FontFamily _defaultUiFamily;

	private readonly FontFamily? _originalUiFamily;

	private readonly FontFamily? _originalDisplayFamily;

	public bool LoadedPrivateFonts => _faces.Count > 0;

	public IReadOnlyList<string> LoadedFontDescriptions => _faces.Select((LoadedFontFace face) => face.Family.Name + " (" + face.SourceName + ")").ToList();

	private FontLibrary(FontFamily fallbackFamily, IEnumerable<LoadedFontFace> faces)
	{
		_fallbackFamily = fallbackFamily;
		_defaultUiFamily = ResolveInstalledFamily("Segoe UI") ?? fallbackFamily;
		_faces.AddRange(faces);
		_originalUiFamily = PickFamily("Arial", "29_") ?? ResolveInstalledFamily("Arial") ?? _defaultUiFamily;
		_originalDisplayFamily = PickFamily("Flintstone", "96_") ?? PickFamily("flintstone", "96_") ?? PickFamily("Flintstone", null) ?? PickFamily("flintstone", null);
	}

	public static FontLibrary LoadFromAssets()
	{
		FontFamily fontFamily = SystemFonts.MessageBoxFont.FontFamily;
		List<LoadedFontFace> list = new List<LoadedFontFace>();
		string assetDirectory = GetAssetDirectory("fonts");
		if (Directory.Exists(assetDirectory))
		{
			foreach (string item in (from path in Directory.EnumerateFiles(assetDirectory, "*.*", SearchOption.AllDirectories)
				where path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)
				select path).OrderBy((string path) => path, StringComparer.OrdinalIgnoreCase))
			{
				try
				{
					PrivateFontCollection privateFontCollection = new PrivateFontCollection();
					privateFontCollection.AddFontFile(item);
					FontFamily[] families = privateFontCollection.Families;
					int num = 0;
					if (num < families.Length)
					{
						FontFamily family = families[num];
						list.Add(new LoadedFontFace
						{
							SourceName = Path.GetFileName(item),
							Collection = privateFontCollection,
							Family = family
						});
						privateFontCollection = null;
					}
					privateFontCollection?.Dispose();
				}
				catch
				{
				}
			}
		}
		return new FontLibrary(fontFamily, list);
	}

	public Font CreateUiFont(float size, FontStyle style = FontStyle.Regular)
	{
		return CreateFont(_originalUiFamily ?? _defaultUiFamily, size, style);
	}

	public Font CreateDisplayFont(float size, FontStyle style = FontStyle.Bold)
	{
		return CreateFont(_originalDisplayFamily ?? _defaultUiFamily, size, style);
	}

	public Font CreateFallbackFont(float size, FontStyle style = FontStyle.Regular)
	{
		return CreateFont(_defaultUiFamily, size, style);
	}

	private FontFamily? PickFamily(string familyName, string? preferredFileToken)
	{
		IEnumerable<LoadedFontFace> source = _faces.Where((LoadedFontFace face) => string.Equals(face.Family.Name, familyName, StringComparison.OrdinalIgnoreCase));
		if (preferredFileToken != null)
		{
			LoadedFontFace loadedFontFace = source.FirstOrDefault((LoadedFontFace face) => face.SourceName.Contains(preferredFileToken, StringComparison.OrdinalIgnoreCase));
			if (loadedFontFace != null)
			{
				return loadedFontFace.Family;
			}
		}
		return source.FirstOrDefault()?.Family;
	}

	private static FontFamily? ResolveInstalledFamily(string familyName)
	{
		try
		{
			return FontFamily.Families.FirstOrDefault((FontFamily family) => string.Equals(family.Name, familyName, StringComparison.OrdinalIgnoreCase));
		}
		catch
		{
			return null;
		}
	}

	private static Font CreateFont(FontFamily family, float size, FontStyle style)
	{
		try
		{
			return new Font(family, size, style, GraphicsUnit.Point);
		}
		catch
		{
			return new Font(SystemFonts.MessageBoxFont.FontFamily, size, style, GraphicsUnit.Point);
		}
	}

	private static string GetAssetDirectory(params string[] parts)
	{
		foreach (string item in CandidateAssetRoots())
		{
			string text = Path.Combine(new string[2] { item, "assets" }.Concat(parts).ToArray());
			if (Directory.Exists(text))
			{
				return text;
			}
		}
		string text2 = Path.Combine(new string[2]
		{
			AppContext.BaseDirectory,
			"assets"
		}.Concat(parts).ToArray());
		Directory.CreateDirectory(text2);
		return text2;
	}

	private static IEnumerable<string> CandidateAssetRoots()
	{
		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string[] array = new string[2]
		{
			AppContext.BaseDirectory,
			Environment.CurrentDirectory
		};
		foreach (string path in array)
		{
			DirectoryInfo directory = new DirectoryInfo(path);
			int i2 = 0;
			while (directory != null && i2 < 8)
			{
				if (seen.Add(directory.FullName))
				{
					yield return directory.FullName;
				}
				i2++;
				directory = directory.Parent;
			}
		}
	}

	public void Dispose()
	{
		foreach (LoadedFontFace face in _faces)
		{
			face.Dispose();
		}
		_faces.Clear();
	}
}
