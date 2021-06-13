using Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Level.Animations
{
    public class AnimationController : MonoBehaviour
    {
        public PlayableDirector timeline;

        void Start()
        {
            EventManager.onStateChange.AddListener(args =>
            {
                switch (args.newState)
                {
                    case GameState.SelectingSkins:
                        timeline.time = 0f;
                        timeline.Stop();
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
            EventManager.onRespawn.AddListener(args =>
            {
                timeline.time = args.crown.time;
                return args;
            }, Priority.Monitor);
        }
    }
}
