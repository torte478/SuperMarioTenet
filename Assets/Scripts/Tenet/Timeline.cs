using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Timeline : MonoBehaviour
{
    public int DurationSec;
    public int FrameRate;

    private readonly LinkedList<CacheNode> cache = new LinkedList<CacheNode>();
    private readonly List<LinkedList<ISnapshot>> timeline = new List<LinkedList<ISnapshot>>();

    private int totalSnapshotCount;
    private TimeSpan delay;
    private DateTime last;
    private int index;
    private bool stopped;

    public LoopState State { get; private set; }
    public int Direction { get; private set; }

    public Action<int> OnInverted;

    void Start()
    {
        totalSnapshotCount = DurationSec * FrameRate;
        last = DateTime.Now;
        delay = TimeSpan.FromMilliseconds(1000f / FrameRate);

        index = 0;
        for (var i = 0; i < totalSnapshotCount; ++i)
            timeline.Add(new LinkedList<ISnapshot>());

        State = LoopState.Forwarded;
        Direction = 1;
    }

    void FixedUpdate()
    {
        if (stopped || index < 0 || index >= totalSnapshotCount) return;

        var now = DateTime.Now;
        if (now.Subtract(last) < delay) return;

        Play();
        RecordToTimeline(now);

        index += Direction;
        var needInvert = Direction == 1 && index == totalSnapshotCount
                         || Direction == -1 && index == -1;
        if (needInvert)
        {
            needInvert = false;
            Direction *= -1;

            State = State == LoopState.Forwarded
                ? LoopState.Inverted
                : LoopState.Infinite;

            OnInverted?.Invoke(Direction);

            index += Direction;
        }

        last = now;
    }

    private void RecordToTimeline(DateTime now)
    {
        if (State == LoopState.Infinite) return;

        var next = new LinkedList<CacheNode>();

        foreach (var snapshot in cache)
        {
            if (snapshot.Time <= now)
                timeline[index].AddLast(snapshot.Snapshot);
            else
                next.AddLast(snapshot);
        }

        cache.Clear();
        foreach (var node in next)
            cache.AddLast(node);
    }

    private void Play()
    {
        if (State == LoopState.Forwarded) return;

        foreach (var snapshot in timeline[index])
        {
            if (snapshot.Direction == 0 || snapshot.Direction == Direction)
                snapshot.Owner.Play(snapshot);
        }
    }

    public override string ToString()
    {
        return $"{index}/{totalSnapshotCount}";
    }

    public void Record(ISnapshot snapshot)
    {
        if (State == LoopState.Infinite) return;

        var existing = cache.FirstOrDefault(_ => IsEqual(snapshot, _));
        if (existing != null)
            cache.Remove(existing);

        RecordToCache(snapshot, DateTime.Now);
    }

    public void Record(ISnapshot snapshot, float delayMs)
    {
        if (State == LoopState.Infinite) return;

        RecordToCache(snapshot, DateTime.Now.AddMilliseconds(delayMs));
    }

    private void RecordToCache(ISnapshot snapshot, DateTime time)
    {
        new CacheNode
        {
            Time = time,
            Snapshot = snapshot
        }
        ._(cache.AddLast);
    }

    public void FirstInvertStart()
    {
        totalSnapshotCount = index + 1;
    }

    public void Stop()
    {
        stopped = true;
    }

    private bool IsEqual(ISnapshot snapshot, CacheNode node)
    {
        return node.Snapshot.Owner == snapshot.Owner
               && node.Snapshot.Direction == snapshot.Direction
               && node.Time.Subtract(DateTime.Now) < delay;
    }

    private sealed class CacheNode
    {
        public DateTime Time { get; set; }
        public int Direction { get; set; }
        public ISnapshot Snapshot { get; set; }
    }

    public enum LoopState
    {
        Forwarded,
        Inverted,
        Infinite,
    }
}
