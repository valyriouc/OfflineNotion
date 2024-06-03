using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UploadClient;

namespace ProtocolTests;

public class WritingShould
{
    [Fact]
    public void WriteWithoutHeaders()
    {
        PMessage msg = new(POP.DOWNLOAD, "Hello world");

        using MemoryStream stream = new();

        msg.Write(stream);

        stream.Position = 0;

        PMessage actual = PMessage.Read(stream);

        Assert.Equal(POP.DOWNLOAD, actual.Operation);
        Assert.Empty(actual.Headers);
        Assert.Equal("Hello world", actual.Body);
    }

    [Fact]
    public void WriteWithHeaders()
    {
        List<PHeader> headers = new()
        {
            new PHeader("test1","val1"),
            new PHeader("test2","val2"),
        };
        
        PMessage msg = new(POP.DOWNLOAD, headers, "Hello world");

        using MemoryStream stream = new();

        msg.Write(stream);

        stream.Position = 0;

        PMessage actual = PMessage.Read(stream);

        Assert.Equal(POP.DOWNLOAD, actual.Operation);
        Assert.Equal(2, actual.Headers.Count);
        Assert.Equal("Hello world", actual.Body);
    }
}
