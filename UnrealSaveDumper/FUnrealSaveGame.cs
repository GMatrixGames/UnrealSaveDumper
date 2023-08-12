using System;
using System.IO;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using MercuryCommons.Utilities.Extensions;
using Newtonsoft.Json;

namespace FriendlyChess.Framework.Unreal.Parser;

[JsonConverter(typeof(FSaveGameConverter))]
public class FSaveGame
{
    public FSaveGameHeader Header;
    public FStructFallback SaveGameObject;

    public FSaveGame(byte[] archive, string name = "Unnamed save game")
    {
        Header = new FSaveGameHeader();
        Header.Deserialize(new FByteArchive(name, archive, new VersionContainer(EGame.GAME_UE5_1)));
    }

    public FSaveGame(FileStream file) : this(file.ReadToEnd(), file.Name) { }

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