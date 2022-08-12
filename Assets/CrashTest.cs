using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class CrashTest : MonoBehaviour
{
    public void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Debug.LogException(t.Exception);
                    return;
                }

                if (t.Result is Firebase.DependencyStatus.Available)
                {
                    Debug.Log("Firebase Is Redy " + Firebase.FirebaseApp.DefaultInstance);
                }
                else
                {
                    Debug.Log("Firebase dependencies broken: " + t.Result);
                }
            });
    }

    public void RegularException()
    {
        throw new System.Exception("Test exception");
    }

    public async void AsyncException()
    {
        await Task.Delay(100);
        throw new System.Exception("Test exception in async");
    }

    public void HardCrash()
    {
        // Creating mesh from other thread is not safe but this is intentional in this case. 
        // Running Mesh constructor concurrently on different threads will lead to SIGSEGV in Unity Engine

        for (int i = 0; i < 5; i++)
            Task.Run(() => CrashMethod())
                .ContinueWith(t => Debug.LogException(t.Exception), TaskContinuationOptions.OnlyOnFaulted);

        void CrashMethod()
        {
            Debug.Log("Start Generation");

            MeshGeneration();

            Debug.Log("Done Generation");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void MeshGeneration()
        {
            Mesh mesh = new()
            {
                vertices = new Vector3[] { new(0, 1), new(1, 1), new(1, 0), },
                triangles = new int[] { 0, 1, 2 }
            };
        }
    }
}
