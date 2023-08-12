using CUE4Parse.Compression;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.Core.Serialization;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;

namespace UnrealSaveDumper.GameSpecific;

public class FFortSaveGameHeader : FSaveGameHeader
{
    public FArchive Deserialize(byte[] archive, string name)
    {
        var Ar = Deserialize(new FByteArchive(name, archive));

        if (FileTypeTag == 0x44464345 && Ar.Read<uint>() == 0xF619 && Ar.Read<int>() == 1)
        {
            var uncompressedSize = Ar.Read<int>();
            var compressedStart = Ar.Position;
            var uncompressed = new byte[uncompressedSize];
            Compression.Decompress(archive, (int) compressedStart, (int) (archive.Length - compressedStart), uncompressed, 0, uncompressed.Length, CompressionMethod.Zlib);
            Ar = new FByteArchive($"{Ar.Name} (decompressed)", uncompressed);
            FileTypeTag = Ar.Read<uint>();
        }

        if (FileTypeTag != 0x8C1809A0 && FileTypeTag != 0xF057217E && FileTypeTag != 0xA2189DE9)
        {
            throw new ParserException(Ar, $"Not a known Fortnite save game file (0x{FileTypeTag:X8})");
        }

        PackageFileUEVersion = FileTypeTag == 0xA2189DE9 ? Ar.Read<FPackageFileVersion>() : FPackageFileVersion.CreateUE4Version(Ar.Read<int>());
        SavedEngineVersion = new FEngineVersion(Ar);

        Ar.Ver = PackageFileUEVersion;

        SaveGameFileVersion = Ar.Read<ESaveGameFileVersion>();

        var serializeCustomVersions = false;
        if (FileTypeTag != 0xF057217E)
        {
            serializeCustomVersions = Ar.ReadFlag();
            CustomVersionFormat = Ar.Read<ECustomVersionSerializationFormat>();
        }

        CustomVersions = serializeCustomVersions ? new FCustomVersionContainer(Ar, CustomVersionFormat) : new FCustomVersionContainer();

        return Ar;
    }
}