using Pmx.Grpc;

namespace Pmx.Admin.Ui.Components.Pages;

public partial class InputPorts
{
    private List<InputPortSetupPair> _inputPortCollection = [];

    private IList<ListPort> _ports = [];

    private void HandleInputPortSetupChanged(InputPortSetupPair inputPortSetup)
    {
        switch (inputPortSetup)
        {
            case { IsStereo: true, Left: not null, Right: not null } when
                inputPortSetup.Left != inputPortSetup.Right:
                PmxGrpcService.SetupInputPort(inputPortSetup.LeftChannelId,
                    inputPortSetup.Left, inputPortSetup.GroupChannelId);
                PmxGrpcService.SetupInputPort(inputPortSetup.RightChannelId,
                    inputPortSetup.Right, inputPortSetup.GroupChannelId);
                inputPortSetup.IsValid = true;
                inputPortSetup.ErrorMessage = "";
                break;
            case { IsStereo: false, Left: not null }:
                PmxGrpcService.SetupInputPort(inputPortSetup.LeftChannelId,
                    inputPortSetup.Left, inputPortSetup.GroupChannelId);
                PmxGrpcService.ClearInputPort(inputPortSetup.RightChannelId);
                inputPortSetup.IsValid = true;
                inputPortSetup.ErrorMessage = "";
                break;
            case { Right: null, Left: null }:
                PmxGrpcService.ClearInputPort(inputPortSetup.LeftChannelId);
                PmxGrpcService.ClearInputPort(inputPortSetup.RightChannelId);
                inputPortSetup.IsValid = true;
                inputPortSetup.ErrorMessage = "";
                break;
            default:
                inputPortSetup.IsValid = false;
                inputPortSetup.ErrorMessage = "Invalid port setup";
                break;
        }

        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        var inputSetups = await PmxGrpcService.GetInputPortsSetup();
        while (inputSetups.Count < 32)
        {
            var nextChannelId = inputSetups.Count == 0
                ? 1
                : inputSetups.Last()
                    .ChannelId + 1;

            inputSetups.Add(new(null, nextChannelId, 1));
        }

        _inputPortCollection = inputSetups
            .Select((setup, index) => new { Setup = setup, Index = index })
            .GroupBy(x => x.Index / 2)
            .Select(group => (group.ElementAtOrDefault(0)!
                .Setup, group.ElementAtOrDefault(1)!
                .Setup))
            .Select(p =>
            {
                var (left, right) = p;
                var displayChannelId =
                    (uint)(Math.Floor(left.ChannelId / 2.0) + 1);
                var mono = left.PortAlias is null || right.PortAlias is null ||
                           left.PortAlias == right.PortAlias;
                if (mono)
                {
                    var channelAlias = left.PortAlias ?? right.PortAlias;
                    return new(
                        false,
                        channelAlias,
                        channelAlias,
                        left.ChannelId,
                        right.ChannelId,
                        displayChannelId,
                        left.GroupChannelId
                    );
                }

                return new InputPortSetupPair(
                    true,
                    left.PortAlias,
                    right.PortAlias,
                    left.ChannelId,
                    right.ChannelId,
                    displayChannelId,
                    left.GroupChannelId
                );
            })
            .ToList();

        var ports = await PmxGrpcService.GetPorts();
        _ports = ports.Where(p => p is
                { Direction: PortDirection.Out, IsMonitor: false })
            .ToList();
    }

    private record InputPortSetupPair(
        bool IsStereo,
        string? Left,
        string? Right,
        uint LeftChannelId,
        uint RightChannelId,
        uint DisplayChannelId,
        uint GroupChannelId,
        bool IsValid = true,
        string? ErrorMessage = null)
    {
        public string? Left { get; set; } = Left;
        public string? Right { get; set; } = Right;
        public bool IsValid { get; set; } = IsValid;
        public string? ErrorMessage { get; set; } = ErrorMessage;
        public bool IsStereo { get; set; } = IsStereo;
        public uint GroupChannelId { get; set; } = GroupChannelId;
    }
}