using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100; // Santé maximale
    private int currentHealth;   // Santé actuelle

    // Références pour la barre de vie
    public Image healthBar;      // Image de la barre de vie (vert)
    public Image healthBarBackground; // Arrière-plan de la barre de vie

    // Références pour l'écran de défaite
    public GameObject defeatPanel;   // Référence au panel de défaite

    void Start()
    {
        // Initialisation de la santé actuelle
        currentHealth = maxHealth;

        // Si les images de la barre de vie ne sont pas assignées, on les cherche automatiquement
        if (healthBar == null)
        {
            healthBar = GameObject.Find("HealthBar").GetComponent<Image>();
        }

        if (healthBarBackground == null)
        {
            healthBarBackground = GameObject.Find("HealthBarBackground").GetComponent<Image>();
        }

        // Si le panel de défaite n'est pas assigné, on le cherche automatiquement
        if (defeatPanel == null)
        {
            defeatPanel = GameObject.Find("DefeatPanel");
        }

        // Cacher le panel de défaite au début
        defeatPanel.SetActive(false);

        // Initialisation de la barre de vie
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0; // S'assurer que la vie ne soit pas inférieure à 0

        // Mettre à jour la barre de vie
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        // Calculer la largeur de la barre de vie en fonction de la santé actuelle
        float healthPercentage = (float)currentHealth / maxHealth; // Pourcentage de vie restant
        healthBar.rectTransform.localScale = new Vector3(healthPercentage, 1f, 1f); // Modifier l'échelle du rectangle de la barre de vie

        // (Optionnel) Change la couleur de la barre de vie en fonction de la santé restante
        if (healthPercentage > 0.5f)
        {
            healthBar.color = Color.green;
        }
        else if (healthPercentage > 0.2f)
        {
            healthBar.color = Color.yellow;
        }
        else
        {
            healthBar.color = Color.red;
        }
    }

    void Die()
    {
        Debug.Log("Le joueur est mort !");
        // Afficher le panel de défaite
        defeatPanel.SetActive(true);

        // (Optionnel) Désactiver le joueur ou les contrôles ici
        // Par exemple : gameObject.SetActive(false); pour désactiver le joueur
    }
}