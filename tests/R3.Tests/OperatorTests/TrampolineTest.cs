using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R3.Tests.OperatorTests;

public class TrampolineTest
{
    [Fact]
    public void NonTrampoline()
    {
        var sender = new Subject<string>();
        var receiver = sender.Share();

        var log = new List<string>();

        // A
        receiver.Subscribe(x =>
        {
            log.Add($"A : {x}");
            if (x == "OnPlayerJoined") sender.OnNext("OnPlayerAddTeam");
        });

        // B
        receiver.Subscribe(x => log.Add($"B : {x}"));

        // C
        receiver.Subscribe(x => log.Add($"C : {x}"));

        sender.OnNext("OnPlayerJoined");

        var msg = string.Join(Environment.NewLine, log);

        msg.Should().Be("""
A : OnPlayerJoined
A : OnPlayerAddTeam
B : OnPlayerAddTeam
C : OnPlayerAddTeam
B : OnPlayerJoined
C : OnPlayerJoined
""");
    }

    [Fact]
    public void Trampoline()
    {
        var sender = new Subject<string>();
        var receiver = sender.Trampoline().Share();

        var log = new List<string>();

        // A
        receiver.Subscribe(x =>
        {
            log.Add($"A : {x}");
            if (x == "OnPlayerJoined") sender.OnNext("OnPlayerAddTeam");
        });

        // B
        receiver.Subscribe(x => log.Add($"B : {x}"));

        // C
        receiver.Subscribe(x => log.Add($"C : {x}"));

        sender.OnNext("OnPlayerJoined");

        var msg = string.Join(Environment.NewLine, log);

        msg.Should().Be("""
A : OnPlayerJoined
B : OnPlayerJoined
C : OnPlayerJoined
A : OnPlayerAddTeam
B : OnPlayerAddTeam
C : OnPlayerAddTeam
""");
    }


    [Fact]
    public void TrampolineIsReusable()
    {
        var sender = new Subject<string>();
        var receiver = sender.Trampoline().Share();

        var log = new List<string>();

        // A
        receiver.Subscribe(x =>
        {
            log.Add(x);
            if (x == "OnPlayerJoined") sender.OnNext("OnPlayerAddTeam");
        });

        sender.OnNext("OnPlayerJoined");

        var msg = string.Join(Environment.NewLine, log);

        msg.Should().Be("""
                        OnPlayerJoined
                        OnPlayerAddTeam
                        """);

        // reset logs
        log.Clear();

        // send again
        sender.OnNext("OnPlayerJoined");

        msg = string.Join(Environment.NewLine, log);
        msg.Should().Be("""
                        OnPlayerJoined
                        OnPlayerAddTeam
                        """);
    }
}
