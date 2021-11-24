using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;

public class CubesManager : MonoBehaviour
{
    public List<Transform> AllCubes;
    public int Xcount = 100, Zcount = 100;
    public float Spacing = 1.2f;
    public Transform[] AllCubesArray;
    private TransformAccessArray transformAccessArray;

    public float Speed = 4f;
    public float Amplitude = 2f;
    public float Period = 16f;
    public bool UseJobs;

    private static readonly ProfilerMarker ProfilerWithJobs =
        new ProfilerMarker(ProfilerCategory.Scripts, "JobsTester.WithJobs");
    private static readonly ProfilerMarker ProfilerNoJobs =
        new ProfilerMarker(ProfilerCategory.Scripts, "JobsTester.NoJobs");    

    [BurstCompile]
    public struct MoveCubesJob : IJobParallelForTransform
    {
        public float realtimeSinceStartup, amplitude, period, speed;
        public void Execute(int index, TransformAccess transform)
        {
            var c = transform.position;
            var offset = Mathf.Sin(((Mathf.PI * 2f) / period) * ((realtimeSinceStartup * speed) + (c.x + c.z))) * amplitude;
            transform.position = new Vector3(c.x, offset, c.z);     
        }
    }

    void Start()
    {
        for (int i = 0; i < Zcount; i++)
        {
            for (int j = 0; j < Xcount; j++)
            {
                var cubeTransf = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                cubeTransf.position = new Vector3(Spacing * j, 0, Spacing * i);
                cubeTransf.parent = transform;
                AllCubes.Add(cubeTransf);
            }
        }
        AllCubesArray = AllCubes.ToArray();
        transformAccessArray = new TransformAccessArray(AllCubesArray, 4);
    }

    void Update()
    {
        if (UseJobs)
        {
            using (ProfilerWithJobs.Auto())
            {
                //1. Criar new Job
                var moveCubesJob = new MoveCubesJob()
                {
                    realtimeSinceStartup = Time.realtimeSinceStartup,
                    amplitude = Amplitude,
                    period = Period,
                    speed = Speed
                };
                //2. Criar o JobHandle
                //var transforms = new TransformAccessArray(AllCubesArray, 4);
                var jobHandle = moveCubesJob.Schedule(transformAccessArray);
                //3. Executar o JobHandle (Complete)
                jobHandle.Complete();
            }
        }
        else
        {
            using (ProfilerNoJobs.Auto())
            {
                foreach (var cube in AllCubes)
                {
                    var c = cube.position;
                    var offset =
                        Mathf.Sin(((Mathf.PI * 2f) / Period) * ((Time.realtimeSinceStartup * Speed) + (c.x + c.z))) *
                        Amplitude;
                    cube.position = new Vector3(c.x, offset, c.z);
                }
            }
        }
    }

    private void OnDestroy()
    {
        transformAccessArray.Dispose();
    }
}
