using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mapper;
using mapperd.Model;

namespace mapperd.Jobs;

public class PollJob(ConnectionManager _mgr, Graph _graph, JsonSerializerOptions _jOpts) : IHostedService
{
    private bool _running = true;
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var thread = new Thread(Run)
        {
            IsBackground = true
        };
        thread.Start();
        return Task.CompletedTask;
    }

    void Run()
    {
        while (_running)
        {
            _graph.Poll();
            foreach (var session in _mgr.Sessions.ToImmutableDictionary())
            {
                foreach (var device in session.Value.Devices)
                {
                    device.Value
                        .Poll(-1);
                }

                // check for changing signals
                foreach (var signal in session.Value.Signals)
                {
                    var flags = signal.Value.Signal.FetchStatus();
                    if (flags.HasFlag(Signal.StatusFlags.SetRemote))
                    {
                        var data = new SignalData
                        {
                            SignalId = signal.Key.ToString(),
                            Value = JsonValue.Create(signal.Value.Signal.GetValue().Item1)
                        };
                        _mgr.QueueOutgoingMessage(session.Key,
                            new Message
                            {
                                Op = OpCode.SignalData,
                                Data = JsonSerializer.SerializeToNode(data, _jOpts)
                            });
                    }
                }
            }

            Thread.Sleep(100);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        return Task.CompletedTask;
    }
}