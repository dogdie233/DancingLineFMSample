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
            EventManager.OnStateChange.AddListener(args =>
            {
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
                return args;
            }, Priority.Monitor);
            EventManager.OnRespawn.AddListener(args =>
            {
                timeline.time = args.crown.Time;
                timeline.Evaluate();
                return args;
            }, Priority.Monitor);
        }
    }
}
