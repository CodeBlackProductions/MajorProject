using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RBush;

public class RTree_Object : ISpatialData
{
    public RTree_Object(Envelope envelope) =>
      _envelope = envelope;

    private readonly Envelope _envelope;
    public ref readonly Envelope Envelope => ref _envelope;

    public GameObject Object { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var other = (RTree_Object)obj;

        double x = Envelope.MinX;
        double y = Envelope.MinY;
        return x == other.Envelope.MinX && y == other.Envelope.MinY;
    }

    public override int GetHashCode()
    {
        return (Envelope.MinX, Envelope.MinY).GetHashCode();
    }
}