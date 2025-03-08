using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Tooltip("La vitesse de rotation pour simuler le cycle de jour et de nuit.")]
    public float rotationSpeed = 6.0f;

    public Material skyboxDay;
    public Material skyboxNight;
    private bool isDay = true;

    public GameObject Moon;

    private int OldNulber = 0;

    void Update()
    {
        // Fait tourner la lumière autour de l'axe X (comme un soleil autour de l'horizon)
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        Moon.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);

        if((int)Moon.transform.rotation.eulerAngles.x != OldNulber)
        {
            if ((int)Moon.transform.rotation.eulerAngles.x % 180 == 0)
            {
                isDay = !isDay;
            }

            if (isDay)
            {
                RenderSettings.skybox = skyboxDay;
            }
            else
            {
                RenderSettings.skybox = skyboxNight;
            }
            OldNulber = (int)Moon.transform.rotation.eulerAngles.x;
        }
        
    }
}
