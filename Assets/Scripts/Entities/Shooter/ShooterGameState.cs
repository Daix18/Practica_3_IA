using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variables/Shooter Game State")]
public class ShooterGameState : ScriptableObject, ISerializationCallbackReceiver
{

    public int NumberOfIterations;

    public int Population;

    public float Grade;
    public float Strength;

    
    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {

    }


}