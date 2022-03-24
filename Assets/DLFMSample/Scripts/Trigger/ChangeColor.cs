using Level.Animations;
using UnityEngine;

namespace Level.Triggers
{
    public class ChangeColor : MonoBehaviour
    {
        [SerializeField] private Material material = null;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private float duration = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (LevelChangeMaterialManager.Instance == null)
				{
                    GameController.Instance.gameObject.AddComponent<LevelChangeMaterialManager>();
				}
                LevelChangeMaterialManager.Instance.ChangeMaterialColor(material, color, duration);
            }
        }
    }
}
