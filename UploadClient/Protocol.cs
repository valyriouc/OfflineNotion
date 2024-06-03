using System.Text;

namespace UploadClient;


/// <summary>
/// Byte ordering
/// </summary>
enum PB : byte
{
    ENQ = 0x05, // Anfrage/Start,
    ETX = 0x03, // Ende der Nachricht
    US = 0x1F, // Einheitentrenner/Blocktrenner
}

/// <summary>
/// Protocol operation
/// </summary>
enum POP : byte
{
    UPLOAD,
    DOWNLOAD
}

// ENQ operation *length+header US body ETX 
struct PHeader
{
    public string Key { get; init; }

    public string Content { get; init; }

    public PHeader(string key, string content)
    {
        Key = key;
        Content = content;
    }

    public void Write(Stream stream)
    {
        byte[] keyB = Encoding.UTF8.GetBytes(Key); 
        byte[] valueB = Encoding.UTF8.GetBytes(Content);

        int length = keyB.Length + 1 + valueB.Length;

        stream.Write(BitConverter.GetBytes(length));
        stream.Write(keyB);
        stream.WriteByte((byte)'=');
        stream.Write(valueB);
    }

    public static PHeader Read(Stream stream)
    {
        byte[] lenBytes = new byte[4];
        stream.ReadExactly(lenBytes);

        int length = BitConverter.ToInt32(lenBytes);

        byte[] buffer = new byte[length];
        stream.ReadExactly(buffer);

        string content = Encoding.UTF8.GetString(buffer);
        string[] split = content.Split("=");

        if (split.Length != 2)
        {
            throw new Exception("Invalid header!");
        }

        return new PHeader(split[0], split[1]);
    }
}

internal class PMessage
{
    public POP Operation { get; init; }

    public List<PHeader> Headers { get; init; }

    public string Body { get; set; }

    public PMessage(POP operation, string body)
        : this(operation, Enumerable.Empty<PHeader>(), body)
    {

    }

    public PMessage(
        POP operation, 
        IEnumerable<PHeader> headers, 
        string body)
    {
        Operation = operation;
        Headers = new List<PHeader>(headers);
        Body = body;
    }

    public void Write(Stream stream)
    {
        stream.WriteByte((byte)PB.ENQ);
        stream.WriteByte((byte)Operation);

        foreach (PHeader header in Headers)
        {
            header.Write(stream);
        }

        stream.WriteByte((byte)PB.US);
        byte[] body = Encoding.UTF8.GetBytes(Body);
        stream.Write(body);

        stream.WriteByte((byte)PB.ETX);

        stream.Flush();
    }

    public static PMessage Read(Stream stream)
    {
        PB start = (PB)stream.ReadByte();

        if (start != PB.ENQ)
        {
            throw new Exception("Not a protocol message!");
        }

        POP operation = (POP)stream.ReadByte();
        if (!Enum.IsDefined(typeof(POP), operation))
        {
            throw new Exception("Not a valid operation");
        }

        while(true)
        {
            PHeader pheader = 
        }
    }
}
