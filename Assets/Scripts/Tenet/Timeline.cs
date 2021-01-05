using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Timeline : MonoBehaviour
{
    public int DurationSec;
    public int FrameRate;

    private readonly LinkedList<ISnapshot> cache = new LinkedList<ISnapshot>();
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
            foreach (var snapshot in cache)
                current.AddLast(snapshot);

            timeline.Add(current);
            cache.Clear();
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

        var existing = cache.FirstOrDefault(_ => _.Owner == snapshot.Owner);
        if (existing != null)
            cache.Remove(existing);

        cache.AddLast(snapshot);
    }

    public void Invert(bool recording)
    {
        needInvert = true;
        recordAfterInvert = recording;
    }
}
