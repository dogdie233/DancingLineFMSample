using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Level
{
    public class LevelReload : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
			{
                GameController.State = GameState.SelectingSkins;
			}
        }
    }
}
