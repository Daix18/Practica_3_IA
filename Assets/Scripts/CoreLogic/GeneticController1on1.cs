using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class GeneticController1on1 : AIController
{
    public void Start()
    {
        Debug.Log("Start Genetic");
    }

    
    protected override void Think()
    {
        _attackToDo = ScriptableObject.CreateInstance<Attack>();
        _attackToDo.AttackMade = _player.Attacks[0];
        _attackToDo.Source = _player;
        _attackToDo.Target = GameState.ListOfPlayers.Players[_player.EnemyId];
       
    }
}

