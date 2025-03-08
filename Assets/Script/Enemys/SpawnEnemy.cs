using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject enemyPrefab; // Le prefab de l’ennemi
    public float spawnInterval = 3.0f; // Intervalle de temps entre chaque apparition
    public Vector3 spawnArea = new Vector3(10, 0, 10); // Zone de spawn
    public float maxSpawnAttempts = 5; // Nombre max d'essais pour trouver un point valide sur le NavMesh
    public float spawnRadiusCheck = 1.0f; // Rayon de la vérification sur le NavMesh

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            EnemySpawn();
            timer = 0;
        }
    }

    void EnemySpawn()
    {
        bool validSpawn = false;
        Vector3 spawnPosition = Vector3.zero;
        int attempts = 0;

        while (!validSpawn && attempts < maxSpawnAttempts)
        {
            // Calculer une position aléatoire dans la zone de spawn
            Vector3 randomPosition = transform.position + new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                spawnArea.y,
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            // Vérifier si la position est sur le NavMesh
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, spawnRadiusCheck, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
                validSpawn = true;
            }

            attempts++;
        }

        // Si une position valide est trouvée, spawn l'ennemi
        if (validSpawn)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Impossible de trouver une position valide pour spawn l'ennemi.");
        }
    }

    // Dessiner la zone de spawn dans l'éditeur
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Couleur rouge semi-transparente
        Gizmos.DrawCube(transform.position, spawnArea);
    }
}

