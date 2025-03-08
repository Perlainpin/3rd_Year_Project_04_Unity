using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 10;       // Dégâts infligés
    public float lifeTime = 2f;   // Durée de vie de la balle

    private void Start()
    {
        Destroy(gameObject, lifeTime); // Détruit la balle après un certain temps
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Vérifie si la balle touche le joueur
        {
            PlayerStats playerHealth = collision.gameObject.GetComponent<PlayerStats>();
            if (playerHealth != null)
            {
                Debug.Log("Player hit");
                playerHealth.TakeDamage(damage); // Applique des dégâts au joueur
            }
            Destroy(gameObject); // Détruit la balle après l'impact
        }
    }
}
