using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using UnrealSaveDumper.GameSpecific;

namespace UnrealSaveDumper;

[JsonConverter(typeof(FSaveGameConverter))]
public class FSaveGame
{
    public FSaveGameHeader Header;
    public FStructFallback SaveGameObject;

    public FSaveGame(byte[] archive, IFileProvider provider, string gameName = "", string name = "Unnamed save game")
    {
        Header = new FSaveGameHeader();
        var Ar = Header.Deserialize(new FByteArchive(name, archive), gameName);
        if (Ar.Game == EGame.GAME_StateOfDecay2) return; // State of Decay 2 has some *issues*
        var proxyArchive = new FObjectAndNameAsStringProxyArchive(Ar, new EmptyPackage("nothing", provider, provider.MappingsForGame));
        SaveGameObject = new FStructFallback(proxyArchive, "None");
    }

    public FSaveGame(FileStream file, IFileProvider provider) : this(file.ReadToEnd(), provider, file.Name) { }

    public FSaveGame()
    {
        Header = null;
        SaveGameObject = null;
    }
}

public class FSaveGameConverter : JsonConverter<FSaveGame>
{
    public override void WriteJson(JsonWriter writer, FSaveGame value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Header");
        serializer.Serialize(writer, value.Header);

        if (value.SaveGameObject != null)
        {
            writer.WritePropertyName("SaveGame");
            serializer.Serialize(writer, value.SaveGameObject);
        }

        writer.WriteEndObject();
    }

    public override FSaveGame ReadJson(JsonReader reader, Type objectType, FSaveGame existingValue, bool hasExistingValue, JsonSerializer serializer)
        => throw new NotImplementedException();
}