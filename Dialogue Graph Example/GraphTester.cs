using DefaultNamespace.Scenes;
using GraphFramework;
using UnityEngine;

namespace DefaultNamespace
{
    public class GraphTester : MonoBehaviour
    {
        public GraphEvaluator executor;
        
        private void Start()
        {
            executor.Initialize();
        }

        private float lastTime = 0f;
        private void Update()
        {
            if (Dialogue.AwaitingInput)
                return;

            if ((Time.unscaledTime - lastTime < 0.50f)) return;
            executor.Step();
            lastTime = Time.unscaledTime;
        }
    }
}