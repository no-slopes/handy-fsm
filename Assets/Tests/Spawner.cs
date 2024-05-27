using System.Collections;
using System.Collections.Generic;
using HandyFSM;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public HandyFSMBrain prefab;
    public TriggerTest test;

    // Start is called before the first frame update
    void Start()
    {
        var brain = Instantiate(prefab, transform.position, Quaternion.identity);
        test.SetBrain(brain);
    }
}
