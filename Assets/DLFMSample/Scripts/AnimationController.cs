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
                    case Level.GameState.Playing:
                        timeline.Play();
                        break;
                    case Level.GameState.WaitingRespawn:
                        timeline.Pause();
                        break;
                }
                return args;
            }, Priority.Lowest);
            EventManager.onRespawn.AddListener(args =>
            {
                timeline.time = args.crown.time;
                return args;
            }, Priority.Lowest);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
