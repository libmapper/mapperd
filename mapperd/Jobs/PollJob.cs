using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
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
                        .Poll(10);
                }

                // check for changing signals
                foreach (var signal in session.Value.Signals)
                {
                    var flags = signal.Value.Signal.GetStatus();
                    if (flags.HasFlag(Signal.Status.RemoteUpdate))
                    {
                        var data = new SignalData
                        {
                            SignalId = signal.Key,
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
            
            DestroyOrphaned();

            Thread.Sleep(100);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _running = false;
        return Task.CompletedTask;
    }
    
    
    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void mpr_dev_free(IntPtr dev);
    [DllImport("mapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern void mpr_sig_free(IntPtr sig);

    private void DestroyOrphaned()
    {
        var now = DateTime.Now;
        var toRemove = new System.Collections.Generic.List<string>();
        foreach (var session in _mgr.Sessions)
            if (session.Value.DestructionTime != null && session.Value.DestructionTime < now)
                toRemove.Add(session.Key);

        lock (_mgr.Sessions)
        {
            var sigNativePtr = typeof(Signal).GetProperty("NativePtr");
            var devNativePtr = typeof(Device).GetProperty("NativePtr");
            foreach (var id in toRemove)
            {
                foreach (var sig in _mgr.Sessions[id].Signals.Values)
                {
                    mpr_sig_free(sig.Signal.NativePtr);
                    sigNativePtr.SetValue(sig.Signal, IntPtr.Zero);
                    GC.SuppressFinalize(sig.Signal);
                }
                foreach (var dev in _mgr.Sessions[id].Devices.Values)
                {

                    mpr_dev_free(dev.NativePtr);
                    devNativePtr.SetValue(dev, IntPtr.Zero);
                    GC.SuppressFinalize(dev);
                }
            
                _mgr.Sessions.Remove(id);
                Console.WriteLine($"Destroyed session {id}");
            }
        }
        
        GC.Collect();
    }
}