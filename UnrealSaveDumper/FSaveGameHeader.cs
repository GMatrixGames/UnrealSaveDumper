using System;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.Core.Serialization;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace FriendlyChess.Framework.Unreal.Parser;

public enum ESaveGameFileVersion
{
    InitialVersion = 1,

    // serializing custom versions into the savegame data to handle that type of versioning
    AddedCustomVersions = 2,

    // added a new UE5 version number to FPackageFileSummary
    PackageFileSummaryVersionChange = 3,

    // -----<new versions can be added above this line>-------------------------------------------------
    VersionPlusOne,
    LatestVersion = VersionPlusOne - 1
}

[JsonConverter(typeof(FSaveGameHeaderConverter))]
public class FSaveGameHeader
{
    private const int UE_SAVEGAME_FILE_TYPE_TAG = 0x53415647;
    protected uint FileTypeTag;
    public ESaveGameFileVersion SaveGameFileVersion = 0;
    public FPackageFileVersion PackageFileUEVersion;
    public FEngineVersion SavedEngineVersion;
    public ECustomVersionSerializationFormat CustomVersionFormat = ECustomVersionSerializationFormat.Unknown;
    public FCustomVersionContainer CustomVersions = new();
    public string SaveGameClassName;

    public void Deserialize(FArchive Ar)
    {
        FileTypeTag = Ar.Read<uint>();

        if (FileTypeTag != UE_SAVEGAME_FILE_TYPE_TAG)
        {
            if (FileTypeTag != 0x44464345 && FileTypeTag != 0x8C1809A0 && FileTypeTag != 0xF057217E && FileTypeTag != 0xA2189DE9) Ar.Position = 0;
            SaveGameFileVersion = ESaveGameFileVersion.InitialVersion;
            return;
        }

        SaveGameFileVersion = Ar.Read<ESaveGameFileVersion>();
        PackageFileUEVersion = SaveGameFileVersion >= ESaveGameFileVersion.PackageFileSummaryVersionChange ? Ar.Read<FPackageFileVersion>() : FPackageFileVersion.CreateUE4Version(Ar.Read<int>());
        SavedEngineVersion = new FEngineVersion(Ar);

        Ar.Ver = PackageFileUEVersion;

        if (SaveGameFileVersion >= ESaveGameFileVersion.AddedCustomVersions)
        {
            CustomVersionFormat = Ar.Read<ECustomVersionSerializationFormat>();
            CustomVersions = new FCustomVersionContainer(Ar, CustomVersionFormat);
        }

        SaveGameClassName = Ar.ReadFString();
    }
}

public class FSaveGameHeaderConverter : JsonConverter<FSaveGameHeader>
{
    public class PackageVersions
    {
        [JsonProperty] public int fileVersionUE4;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int fileVersionUE5;
    }

    public override void WriteJson(JsonWriter writer, FSaveGameHeader value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("PackageFileUEVersion");
        serializer.Serialize(writer, new PackageVersions
        {
            fileVersionUE4 = value.PackageFileUEVersion.FileVersionUE4,
            fileVersionUE5 = value.PackageFileUEVersion.FileVersionUE5
        });

        writer.WritePropertyName("SavedEngineVersion");
        writer.WriteValue(value.SavedEngineVersion.ToString());

        writer.WritePropertyName("SaveGameFileVersion");
        writer.WriteValue(value.SaveGameFileVersion);

        writer.WritePropertyName("SaveGameClassName");
        writer.WriteValue(value.SaveGameClassName);

        if (value.CustomVersions is { Versions.Length: > 0 })
        {
            serializer.Serialize(writer, value.CustomVersions);
        }

        writer.WriteEndObject();
    }

    public override FSaveGameHeader ReadJson(JsonReader reader, Type objectType, FSaveGameHeader existingValue, bool hasExistingValue, JsonSerializer serializer)
        => throw new NotImplementedException();
}