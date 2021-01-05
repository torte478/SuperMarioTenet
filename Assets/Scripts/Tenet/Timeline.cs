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
    private bool needInvert;
    private bool recordAfterInvert;

    public bool Recording { get; private set; }
    public bool Playing { get; private set; }
    public int Direction { get; private set; }

    public Action<int> OnInverted;

    void Start()
    {
        totalSnapshotCount = DurationSec * FrameRate;
        last = DateTime.Now;
        delay = TimeSpan.FromMilliseconds(1000f / FrameRate);
        index = 0;
        timeline.Capacity = totalSnapshotCount;
        Recording = true;
        Playing = false;
        Direction = 1;
    }

    void FixedUpdate()
    {
        if (index < 0 || index >= totalSnapshotCount) return;

        var now = DateTime.Now;
        if (now.Subtract(last) < delay) return;

        if (Recording)
        {
            var current = new LinkedList<ISnapshot>();
            var next = new LinkedList<CacheNode>();

            foreach (var snapshot in cache)
            {
                if (snapshot.Time <= now)
                    current.AddLast(snapshot.Snapshot);
                else
                    next.AddLast(snapshot);
            }

            timeline.Add(current);

            cache.Clear();
            foreach (var node in next)
                cache.AddLast(node);
        }

        if (Playing)
        {
            foreach (var snapshot in timeline[index])
                snapshot.Owner.Play(snapshot);
        }

        if (needInvert)
        {
            needInvert = false;
            Direction *= -1;
            Recording = recordAfterInvert;
            Playing = true;
            OnInverted?.Invoke(Direction);
        }
        else
        {
            index += Direction;
        }

        last = now;
    }

    public override string ToString()
    {
        return $"{index}/{totalSnapshotCount}";
    }

    public void Record(ISnapshot snapshot)
    {
        if (!Recording) return;

        var existing = cache.FirstOrDefault(_ => IsEqual(snapshot, _));
        if (existing != null)
            cache.Remove(existing);

        new CacheNode
        {
            Time = DateTime.Now,
            Snapshot = snapshot
        }
        ._(cache.AddLast);
    }

    public void Record(ISnapshot snapshot, float delayMs)
    {
        if (!Recording) return;

        new CacheNode
        {
            Time = DateTime.Now.AddMilliseconds(delayMs),
            Snapshot = snapshot
        }
        ._(cache.AddLast);
    }

    public void Invert(bool recording)
    {
        needInvert = true;
        recordAfterInvert = recording;
    }

    private bool IsEqual(ISnapshot snapshot, CacheNode node)
    {
        return node.Snapshot.Owner == snapshot.Owner 
               && node.Time.Subtract(DateTime.Now) < delay;
    }

    private sealed class CacheNode
    {
        public DateTime Time { get; set; }
        public ISnapshot Snapshot { get; set; }
    }
}
