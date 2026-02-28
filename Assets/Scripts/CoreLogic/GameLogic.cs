//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography.X509Certificates;
//using UnityEngine;

//public class GameLogic : MonoBehaviour
//{
//    public PlayerList PlayerList;
//    public GameState GameState;


//    public GameEvent EndGameEvent;
//    public AttackResultEvent AttackResult;
//    public PlayerEvent ChangeTurnEvent;

//    private int _count = 0;
//    public IEnumerator Start()
//    {
//        yield return new WaitForEndOfFrame();
//        GameState.IsFinished = false;
//        ChangeTurn();
//    }


//    public void ChangeTurn()
//    {
//        var next = _count;
//        _count = (_count + 1) % 2;
//        GameState.CurrentPlayer = PlayerList.Players[next];
//        ChangeTurnEvent.Raise(PlayerList.Players[next]);


//    }

//    private bool EndGameTest()
//    {
//        if (PlayerList.Players.Any(p => p.HP <= 0))
//        {
//            GameState.IsFinished = true;
//            EndGameEvent.Raise();
//            return true;
//        }
//        return false;
//    }

//    public void OnAttackDone(Attack att)
//    {

//        Debug.Log($"Received Attack {att}");
//        var hitRoll = Dice.PercentageChance();
//        var result = ScriptableObject.CreateInstance<AttackResult>();
//        result.IsHit = false;
//        result.Attack = att;

//        if (result.Attack != null)
//        {
//            result.Energy = att.AttackMade.Energy;
//            if (att.Source.Energy >= att.AttackMade.Energy && hitRoll <= att.AttackMade.HitChance)
//            {
//                result.IsHit = true;

//                result.Damage = Dice.RangeRoll(att.AttackMade.MinDam, att.AttackMade.MaxDam + 1);


//                att.Target.HP -= result.Damage;

//            }

//            if (att.Source.Energy >= att.AttackMade.Energy)
//            {
//                att.Source.Energy -= result.Energy;
//            }

//            Debug.Log($"With Result \n    {result}");
//            AttackResult.Raise(result);
//        }

//        if (!EndGameTest())
//            ChangeTurn();
//    }
//}
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public PlayerList PlayerList; //lista de jugadores 
    public GameState GameState; //estado del juego

    //ScriptableObject Events. Sirven para avisar a otros scripts que algo pasó:
    public GameEvent EndGameEvent;//
    public AttackResultEvent AttackResult;
    public PlayerEvent ChangeTurnEvent;

    private int _count = 0;
    public IEnumerator Start() //corrutina
    {
        yield return new WaitForEndOfFrame(); //espera 1 frame antes de empezar
        GameState.IsFinished = false;//inicializa el juego
        ChangeTurn(); //cambia el turno
    }


    public void ChangeTurn() //cambia el turno entre 2 jugadores
    {
        // NO cambiar turno si el juego ya terminó
        if (EndGameTest()) return;//AÑADIDO

        var next = _count;
        _count = (_count + 1) % 2;
        GameState.CurrentPlayer = PlayerList.Players[next]; //guarda quien juega ahora
        ChangeTurnEvent.Raise(PlayerList.Players[next]); //pra q se actualice la UI


    }

    private bool EndGameTest() //comprueba si algún jugador murió, si es así, termina el juego y avisa a la UI para mostrar el ganador
    {
        if (PlayerList.Players.Any(p => p.HP <= 0))
        {
            GameState.IsFinished = true;

            EvaluateGeneticController(); //AÑADIDO
            Debug.Log(" SI SE EJECUTA");
            EndGameEvent.Raise();
            return true;
        }
        return false;
    }

    public void OnAttackDone(Attack att)
    {

        Debug.Log($"Received Attack {att}");
        var hitRoll = Dice.PercentageChance(); //genera un número entre 0 y 1 para comparar con la probabilidad de acierto del ataque
        var result = ScriptableObject.CreateInstance<AttackResult>(); //crea el resultado del ataque (si fue hit cuanto daño cuanta energia se gasto)
        result.IsHit = false;
        result.Attack = att;

        if (result.Attack != null)
        {
            result.Energy = att.AttackMade.Energy;
            if (att.Source.Energy >= att.AttackMade.Energy && hitRoll <= att.AttackMade.HitChance) //se puede atacar?? suficiente energia y tirado < o = a % de acierto del ataque
            {
                result.IsHit = true;

                result.Damage = Dice.RangeRoll(att.AttackMade.MinDam, att.AttackMade.MaxDam + 1);//calcula daño 


                att.Target.HP -= result.Damage;//resta vida al objetivo

            }

            if (att.Source.Energy >= att.AttackMade.Energy)//aunq falle el ataque si tenia energia suficiente para intentarlo, se gasta la energia
            {
                att.Source.Energy -= result.Energy;
            }

            Debug.Log($"With Result \n    {result}");
            AttackResult.Raise(result); //animaciones, actualiza la UI, etc. dependiendo del resultado del ataque
        }

        if (!EndGameTest())//comprueba si el juego terminó, si no, cambia el turno
            ChangeTurn();
    }

    //AÑADIDO
    private void EvaluateGeneticController()
    {
        //var controllers = FindObjectsByType<GeneticController1on1>(FindObjectsSortMode.None);

        //foreach (var controller in controllers)
        //{
        //    controller.EvaluateFitness();
        //}
        // Buscamos en la escena el objeto que tiene el controlador genético
        // En tu configuración de Unity, esto está en AIControllerMin
        GeneticController1on1 geneticAI = FindFirstObjectByType<GeneticController1on1>();

        if (geneticAI != null)
        {
            geneticAI.EvaluateFitness(); // Llama a la función que ya definiste en tu controlador
            //geneticAI.EvaluateCurrentIndividualFitness();
            Debug.Log($"Fitness de la IA evaluado: {geneticAI.CurrentIndividual.fitness}");
        }
    }
}