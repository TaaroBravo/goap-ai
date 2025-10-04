using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MyGoapAction
{
    
    public List<Func<WorldState,bool>> Preconditions { get; private set; }
    public List<Action<WorldState>> Effects { get; private set; }
    public string zone;

    public string testDebug;
    public MyGoapAction previousAction;

    public string Name { get; private set; }
    public float Cost { get; private set; }

    public MyGoapAction(string name)
    {
        Name = name;
        Cost = 1f;
        Preconditions = new List<Func<WorldState, bool>>();
        Effects = new List<Action<WorldState>>();
    }

    public MyGoapAction SetCost(float cost)
    {
        if (cost < 1f)
            Cost = 1f;
        Cost = cost;
        return this;
    }

    public MyGoapAction Pre(Func<WorldState,bool> value)
    {
        Preconditions.Add(value);
        return this;
    }

    public MyGoapAction Effect(Action<WorldState> value)
    {
        Effects.Add(value);
        return this;
    }
}
