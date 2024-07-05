using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.layer = 8;

            foreach (Transform child2 in child)
            {
                child2.gameObject.layer = 8;
            }
            //Change layer inside the object
        }
    }
}
