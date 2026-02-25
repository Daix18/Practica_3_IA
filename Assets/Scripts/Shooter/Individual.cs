using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
[Serializable]
public class Individual: IComparable<Individual>
{
    public float degree;
    public float horizontal;

    public float strength;
    public float fitness;

    public Individual(float d, float h,float s)
    {
        fitness = float.MaxValue;
        degree = d;
        horizontal = h;
        strength = s;
    }

    public int CompareTo(Individual other)
    {
        return fitness.CompareTo(other.fitness);
    }
}
