using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Ending
{
    public class EndingFinishTrigger : MonoBehaviour
    {
        [SerializeField] private EndingPyramid ending;

        private void Awake()
		{
            ending = ending ?? transform.parent.gameObject.GetComponent<EndingPyramid>();
		}

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) { ending.OnFinishTriggerEnter(); }
        }
    }
}
