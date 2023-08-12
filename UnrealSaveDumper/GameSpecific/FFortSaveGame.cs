using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;
using FriendlyChess.Framework.Fortnite.Parser;

namespace UnrealSaveDumper;

public class FFortSaveGame : FSaveGame
{
    public FFortSaveGame(byte[] archive, IFileProvider provider, string name = "Unnamed save game")
    {
        Header = new FFortSaveGameHeader();
        var Ar = ((FFortSaveGameHeader) Header).Deserialize(archive, name);
        var proxyArchive = new FObjectAndNameAsStringProxyArchive(Ar, new EmptyPackage("nothing", provider, provider.MappingsForGame));
        SaveGameObject = new FStructFallback(proxyArchive, "None");
    }
}

public sealed class EmptyPackage(string name, IFileProvider provider, TypeMappings mappings) : AbstractUePackage(name, provider, mappings)
{
    public override UObject GetExportOrNull(string name, StringComparison comparisonType = StringComparison.Ordinal)
        => throw new NotImplementedException();

    public override ResolvedObject ResolvePackageIndex(FPackageIndex index) => throw new NotImplementedException();

    public override FPackageFileSummary Summary { get; } = new();
    public override FNameEntrySerialized[] NameMap { get; } = Array.Empty<FNameEntrySerialized>();
    public override Lazy<UObject>[] ExportsLazy { get; } = Array.Empty<Lazy<UObject>>();
    public override bool IsFullyLoaded => true;
}