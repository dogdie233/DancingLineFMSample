using Event;
using UnityEngine;
using UnityEngine.Playables;

namespace Level.Animations
{
    public class AnimationController : MonoBehaviour
    {
        public PlayableDirector timeline;

        void Start()
        {
            GameController.Instance.OnStateChange.AddListener(args =>
            {
				if (args.canceled) { return; }
                switch (args.newState)
                {
                    case GameState.SelectingSkins:
                        timeline.time = 0f;
                        timeline.Stop();
                        timeline.Evaluate();
                        break;
                    case GameState.Playing:
                        timeline.Play();
                        break;
                    case GameState.WaitingRespawn:
                    case GameState.GameOver:
                        timeline.Pause();
                        break;
                }
            }, Priority.Monitor);
            GameController.Instance.OnRespawn.AddListener(args =>
            {
                if (args.canceled) { return; }
                timeline.time = BGMController.Time;
                timeline.Evaluate();
            }, Priority.Monitor);
        }
    }
}
