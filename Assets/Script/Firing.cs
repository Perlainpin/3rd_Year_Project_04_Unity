using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firing : MonoBehaviour
{
    public GameObject bullet;
    public float shootForce, upwardForce;
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;

    public int magazineSize, bulletPerTap;

    public bool allowButtonHold;

    public int bulletsLeft, bulletsShot;

    private Rigidbody playerRb;
    public float recoilForce;

    public bool shooting = false, reloading = false, readyToShoot = false;

    private Camera fpsCam;
    public Transform spawnBullet;

    //public GameObject muzzleFlash;

    public bool allowInvoke = true;

    public GameObject Casing;
    public float Casingspeed = 100f;

    public GameObject spawnCasing;
    public GameObject spawnFlash;


    public AudioClip PlayershootSound;
    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerRb = GameObject.Find("Character").GetComponent<Rigidbody>();
        fpsCam = GameObject.Find("Camera").GetComponent<Camera>();
    }

    public void Shoot()
    {
        Debug.Log("shoot");
        readyToShoot = false;
        GetComponent<AudioSource>().PlayOneShot(PlayershootSound); // Joue le son de tir

        GameObject newCasing = Instantiate(Casing, spawnCasing.transform.position, Casing.transform.rotation);
        newCasing.GetComponent<Renderer>().enabled = true;
        Destroy(newCasing, 5f);

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        Vector3 directionWithoutSpread = targetPoint - spawnBullet.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        GameObject currentBullet = Instantiate(bullet, spawnBullet.position, Quaternion.identity);
        currentBullet.GetComponent<Renderer>().enabled = true;

        float maxLifeTime = currentBullet.GetComponent<CustomBullet>().maxLifeTime;
        
        currentBullet.transform.forward = directionWithSpread.normalized;

        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);

        Destroy(currentBullet, maxLifeTime);

        playerRb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
            
        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        if (bulletsShot < bulletPerTap && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    public void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

}
