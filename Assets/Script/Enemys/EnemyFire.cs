using UnityEngine;

public class EnemyFire : MonoBehaviour
{
    public GameObject bulletPrefab; // Le prefab de la balle à tirer
    public Transform firePoint;     // Point de départ de la balle
    public Transform firePoint2;
    public float bulletSpeed = 20f; // Vitesse de la balle
    public AudioClip shootSound;     // Son de tir

    public void Shoot()
    {
        GetComponent<AudioSource>().PlayOneShot(shootSound); // Joue le son de tir
        // Instancie la balle au point de tir et oriente-la vers la cible
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        GameObject bullet2 = Instantiate(bulletPrefab, firePoint2.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bulletSpeed; // Applique la vitesse
        }

        Rigidbody rb2 = bullet2.GetComponent<Rigidbody>();
        if (rb2 != null)
        {
            rb2.velocity = firePoint2.forward * bulletSpeed; // Applique la vitesse
        }
    }
}