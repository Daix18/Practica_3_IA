using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShotgunConfiguration : MonoBehaviour
{
    public float XDegrees;
    public float HorizontalDegrees;
    public float Strength;

    public Rigidbody ShotSpherePrefab;
    public Transform ShotPosition;

    public Transform Target;

    public GeneticAlgorithm Genetic;
    public Individual CurrentIndividual;


    private bool _ready;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 5f;
        Vector3 pos = Target.position;
        pos.x = Random.Range(-5f, 5f);
        Target.position = pos;

        Genetic = new GeneticAlgorithm(
            30, // generaciones
            20, // tamaño población
            GeneticAlgorithm.CrossoverType.Uniform,
            0.1f // mutation rate
        );

        _ready = true;
    }

    public void ShooterConfigure(float degree, float horizontal, float strength)
    {
        XDegrees = degree;
        HorizontalDegrees = horizontal;
        Strength = strength;
    }

    public void GetResult(float data)
    {
        CurrentIndividual.fitness = data;
        Debug.Log($"Gen {Genetic.CurrentGeneration} | Fitness: {data}");
        _ready = true;
    }

    public void Shot()
    {
        _ready = false;

        transform.eulerAngles = new Vector3(XDegrees, HorizontalDegrees, 0);
        var shot = Instantiate(ShotSpherePrefab, ShotPosition);
        shot.gameObject.GetComponent<TargetTrigger>().Target = Target;
        shot.gameObject.GetComponent<TargetTrigger>().OnHitCollider += GetResult;
        shot.isKinematic = false;
        var force = transform.up * Strength;
        shot.AddForce(force,ForceMode.Impulse);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 1f;
            CurrentIndividual = Genetic.GetFittest();
            ShooterConfigure(CurrentIndividual.degree, CurrentIndividual.horizontal, CurrentIndividual.strength);
            Shot();
        }

        if (_ready)
        {
            CurrentIndividual = Genetic.GetNext();
            if (CurrentIndividual != null)
            {
                ShooterConfigure(CurrentIndividual.degree,CurrentIndividual.horizontal, CurrentIndividual.strength);
                Shot();
            }
            else
            {
                Debug.Log("FIN DEL ENTRENAMIENTO");
                CurrentIndividual = Genetic.GetFittest();
                _ready = false;
            }
        }
    }
}
