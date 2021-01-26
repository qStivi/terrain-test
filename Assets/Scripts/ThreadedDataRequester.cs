using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadedDataRequester : MonoBehaviour
{
    private static ThreadedDataRequester instance;
    private readonly Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();


    private void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }


    private void Update()
    {
        if (dataQueue.Count > 0)
            for (var i = 0; i < dataQueue.Count; i++)
            {
                var threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
    }
    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        void ThreadStart()
        {
            instance.DataThread(generateData, callback);
        }

        new Thread(ThreadStart).Start();
    }

    private void DataThread(Func<object> geneateData, Action<object> callback)
    {
        var data = geneateData();
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }
    
    private struct ThreadInfo
    {
        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

        public readonly Action<object> callback;
        public readonly object parameter;
    }

}
