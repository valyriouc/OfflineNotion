using System.Text;

namespace UploadClient;


/// <summary>
/// Byte ordering
/// </summary>
public enum PB : byte
{
    ENQ = 0x05, // Anfrage/Start,
    RS = 0x1E, // Header starting
    US = 0x1F, // Header/body separator 
}

/// <summary>
/// Protocol operation
/// </summary>
public enum POP : byte
{
    UPLOAD,
    DOWNLOAD
}

// ENQ operation *length+header US body ETX 
public struct PHeader
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

public class PMessage
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
            stream.WriteByte((byte)PB.RS);
            header.Write(stream);
        }

        stream.WriteByte((byte)PB.US);
        byte[] body = Encoding.UTF8.GetBytes(Body);
        stream.Write(BitConverter.GetBytes(body.Length));
        stream.Write(body);

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

        List<PHeader> headers = new();
        while (true)
        {
            PB header = (PB)stream.ReadByte();  
            if (header == PB.US)
            {
                break;
            }

            PHeader tmp = PHeader.Read(stream);
            headers.Add(tmp);

        }

        byte[] lenByte = new byte[4];
        stream.ReadExactly(lenByte);

        int length = BitConverter.ToInt32(lenByte);

        byte[] buffer = new byte[length];
        stream.ReadExactly(buffer);

        return new PMessage(operation, headers, Encoding.UTF8.GetString(buffer));
    }
}
