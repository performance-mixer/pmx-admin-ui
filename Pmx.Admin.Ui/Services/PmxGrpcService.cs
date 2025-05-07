using Grpc.Core;
using Pmx.Grpc;

namespace Pmx.Admin.Ui.Services;

public record InputPortSetup(
    string? PortAlias,
    uint ChannelId,
    uint GroupChannelId);

public static class InputPortSetupExtensions
{
    public static IEnumerable<InputPortSetup> FillGaps(
        this IEnumerable<InputPortSetup> setups)
    {
        InputPortSetup? lastSetup = null;
        foreach (var setup in setups)
        {
            var expectedChannelId = lastSetup?.ChannelId + 1 ?? 1;
            var actualChannelId = setup.ChannelId;
            while (expectedChannelId < actualChannelId)
            {
                yield return new(null, expectedChannelId, 1);
                expectedChannelId++;
            }

            yield return setup;
            lastSetup = setup;
        }
    }
}

public interface IPmxGrpcService
{
    Task<IList<ListPort>> GetPorts();
    Task<IList<InputPortSetup>> GetInputPortsSetup();
    Task SetupInputPort(uint channelId, string port, uint groupChannelId);
    AsyncUnaryCall<Response> ClearInputPort(uint channelId);
}

public class PmxGrpcService(PmxGrpc.PmxGrpcClient client) : IPmxGrpcService
{
    public async Task<IList<ListPort>> GetPorts()
    {
        var response = await client.ListPortsAsync(new(), new());
        return response.Ports;
    }

    public async Task<IList<InputPortSetup>> GetInputPortsSetup()
    {
        var response = await client.ListInputPortsSetupAsync(new(), new());
        return response.Setups
            .Select(s =>
                new InputPortSetup(s.Port, s.ChannelId, s.GroupChannelId))
            .ToList();
    }

    public async Task SetupInputPort(uint channelId, string? port,
        uint groupChannelId)
    {
        var request = new SetupInputPortRequest
            { ChannelId = channelId, Port = port, GroupChannelId = groupChannelId};
        await client.SetupInputPortAsync(request);
    }

    public AsyncUnaryCall<Response> ClearInputPort(uint channelId)
    {
        return client.ClearInputPortAsync(new() { ChannelId = channelId });
    }
}