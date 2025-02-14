using Pmx.Admin.Ui.Services;

namespace Pmx.Tests;

public class InputPortSetupExtensionTests
{
    [Fact]
    public void ShouldAllowFillGaps()
    {
        var result = new List<InputPortSetup>
        {
            new("my_port_1", 3, 0),
            new("my_port_2", 4, 0),
            new("my_port_3", 6, 0)
        };

        var channelIds = result.FillGaps()
            .Select(s => s.ChannelId);
        Assert.Equal(new uint[] { 0, 1, 2, 3, 4, 5, 6 }, channelIds);

        var portAlias = result.FillGaps()
            .Select(s => s.PortAlias);
        Assert.Equal([
            null, null, null, "my_port_1", "my_port_2", null, "my_port_3"
        ], portAlias);
    }
}