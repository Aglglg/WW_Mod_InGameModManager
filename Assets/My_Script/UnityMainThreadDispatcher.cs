using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    private static UnityMainThreadDispatcher instance;
    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<UnityMainThreadDispatcher>();
                if (instance == null)
                {
                    // Create a new GameObject to host the dispatcher
                    GameObject dispatcherObject = new GameObject("UnityMainThreadDispatcher");
                    instance = dispatcherObject.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(dispatcherObject); // Make it persistent across scenes
                }
            }
            return instance;
        }
    }

    // Enqueue an action to be executed on the main thread
    public void Enqueue(Action action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        // Execute all actions in the queue
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue().Invoke();
            }
        }
    }
}