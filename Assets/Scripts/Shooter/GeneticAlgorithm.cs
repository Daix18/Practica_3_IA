using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GeneticAlgorithm
{
    
    public List<Individual> population;
    private int _currentIndex;

    public int CurrentGeneration;
    public int MaxGenerations;

    public string Summary;
    public enum CrossoverType
    {
        OnePoint,
        Uniform,
        Arithmetic
    }

    public GeneticAlgorithm(int numberOfGenerations, int populationSize, CrossoverType type, float mutRate)
    {
        CurrentGeneration = 0;
        MaxGenerations = numberOfGenerations;
        crossoverType = type;
        mutationRate = mutRate;
        GenerateRandomPopulation(populationSize);
        Summary = "";
    }
    public void GenerateRandomPopulation(int size)
    {
        population = new List<Individual>();
        for (int i = 0; i < size; i++)
        {
            population.Add(
                new Individual(
                    Random.Range(10f, 80f),     // vertical
                    Random.Range(-45f, 45f),    // horizontal
                    Random.Range(2f, 12f)       // fuerza
                )
            );
        }
        StartGeneration();
    }


    public CrossoverType crossoverType;
    public float mutationRate = 0.02f;

    public Individual GetFittest()
    {
        population.Sort();
        return population[0];
    }


    public void StartGeneration()
    {
        _currentIndex = 0;
        CurrentGeneration ++;
    }
    public Individual GetNext()
    {
        if (_currentIndex == population.Count)
        {
            EndGeneration();
            if (CurrentGeneration >= MaxGenerations)
            {
                Debug.Log(Summary);
                return null;
            }
            StartGeneration();
        }

        return population[_currentIndex++];
    }

    public void EndGeneration()
    {
        population.Sort();
        Summary += $"{GetFittest().fitness};";
        if (CurrentGeneration < MaxGenerations)
        {
            Crossover();
            Mutation();
        }
    }

    public void Crossover()
    {
        var ind1 = population[0];
        var ind2 = population[1];

        Individual new1;
        Individual new2;

        switch (crossoverType)
        {
            case CrossoverType.OnePoint:

                // Cruce en un punto (intercambiamos algunos genes)
                new1 = new Individual(
                    ind1.degree,
                    ind2.horizontal,
                    ind2.strength
                );

                new2 = new Individual(
                    ind2.degree,
                    ind1.horizontal,
                    ind1.strength
                );
                break;

            case CrossoverType.Uniform:

                float degree1 = Random.value < 0.5f ? ind1.degree : ind2.degree;
                float horizontal1 = Random.value < 0.5f ? ind1.horizontal : ind2.horizontal;
                float strength1 = Random.value < 0.5f ? ind1.strength : ind2.strength;

                float degree2 = Random.value < 0.5f ? ind1.degree : ind2.degree;
                float horizontal2 = Random.value < 0.5f ? ind1.horizontal : ind2.horizontal;
                float strength2 = Random.value < 0.5f ? ind1.strength : ind2.strength;

                new1 = new Individual(degree1, horizontal1, strength1);
                new2 = new Individual(degree2, horizontal2, strength2);
                break;

            case CrossoverType.Arithmetic:

                float alpha = Random.value;

                float newDegree = alpha * ind1.degree + (1 - alpha) * ind2.degree;
                float newHorizontal = alpha * ind1.horizontal + (1 - alpha) * ind2.horizontal;
                float newStrength = alpha * ind1.strength + (1 - alpha) * ind2.strength;

                new1 = new Individual(newDegree, newHorizontal, newStrength);
                new2 = new Individual(newDegree, newHorizontal, newStrength);
                break;

            default:
                new1 = ind1;
                new2 = ind2;
                break;
        }

        population.RemoveAt(population.Count - 1);
        population.RemoveAt(population.Count - 1);

        population.Add(new1);
        population.Add(new2);
    }

    public void Mutation()
    {
        foreach (var individual in population)
        {
            if (Random.value < mutationRate)
            {
                individual.degree += Random.Range(-10f, 10f);
            }

            if (Random.value < mutationRate)
            {
                individual.strength += Random.Range(-2f, 2f);
            }

            individual.degree = Mathf.Clamp(individual.degree, 0f, 90f);
            individual.strength = Mathf.Clamp(individual.strength, 0f, 12f);
        }
    }
}
