using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.UObject;

namespace UnrealSaveDumper.GameSpecific;

public class FFortSaveGame : FSaveGame
{
    public FFortSaveGame(byte[] archive, IFileProvider provider, string name = "Unnamed save game")
    {
        Header = new FFortSaveGameHeader();
        var Ar = ((FFortSaveGameHeader) Header).Deserialize(archive, name);
        if (provider.MappingsForGame == null) return; // we need mappings for properties
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
    public override FNameEntrySerialized[] NameMap { get; } = [];
    public override Lazy<UObject>[] ExportsLazy { get; } = [];
    public override bool IsFullyLoaded => true;
}