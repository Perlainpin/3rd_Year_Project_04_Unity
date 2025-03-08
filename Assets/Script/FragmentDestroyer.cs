//using System.Collections;
//using Unity.VisualScripting;
//using UnityEditor.SearchService;
//using UnityEngine;

//public class FragmentDestroyer : MonoBehaviour
//{
//    GameObject[] allObjects;

//    void Awake()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

//        foreach (GameObject go in allObjects)
//        {
//            if (!go.IsDestroyed())
//            {
//                StartCoroutine(DelayAction(go));
//            }
//        }
//    }

//    private void Deprecate(GameObject go)
//    {
//        go.GetComponent<MeshCollider>().enabled = false;
//        go.GetComponent<Rigidbody>().drag = 5;
        
//    }

//    IEnumerator DelayAction(GameObject go)
//    {
//        if (go.name.StartsWith("Fragment"))
//        {

//            yield return new WaitForSeconds(3.5f);

//            Deprecate(go);

//            yield return new WaitForSeconds(8);

//            Destroy(go);
//        }
//        yield return new WaitForSeconds(0);
//    }
//}
