using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Ending
{
    public class EndingOpenTrigger : MonoBehaviour
    {
        [SerializeField] private EndingPyramid ending;

        private void Awake()
		{
            ending = ending ?? transform.parent.gameObject.GetComponent<EndingPyramid>();
		}

        private void OnTriggerEnter() => ending.OnOpenTriggerEnter();
    }
}
