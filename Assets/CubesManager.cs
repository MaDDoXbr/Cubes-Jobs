using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Profiling;

//https://www.khanacademy.org/computing/computer-programming/programming-natural-simulations/programming-oscillations/a/oscillation-amplitude-and-period

public class CubesManager : MonoBehaviour
{
    public List<Transform> AllCubes;
    //private List<Vector3> CubesPositions;
    public int Xcount = 100, Zcount = 100;
    public float Spacing = 1.2f;
    private Transform[] AllCubesArray;

    private  TransformAccessArray transformAccessArray;
 
    static readonly ProfilerMarker ProfilerWithJobs = new ProfilerMarker(ProfilerCategory.Scripts, "JobsTester.WithJobs");
    static readonly ProfilerMarker ProfilerNoJobs = new ProfilerMarker(ProfilerCategory.Scripts, "JobsTester.NoJobs");
    
    //Fields "Jobifiable"
    public float Speed = 4f;
    public float amplitude = 2f, period = 4f;
    public bool UseJobs;

    [BurstCompile]
    public struct MoveCubesJob : IJobParallelForTransform
    {
        public float realtimeSinceStartup, amplitude, period, Speed;
      
        //Será executado 1 vez para cada índice
        public void Execute(int index, TransformAccess transform) //Versão do Transform pro Jobs System
        {
            //transform.position, rotation, etc
            //transform.position = NodePos; 
            var c = transform.position;
            //math.PI,sin tem otimização SIMD, vs mathf.PI,Sin
            var offset =
                math.sin(((math.PI * 2) / period) * ((realtimeSinceStartup * Speed) + (c.x + c.z))) *
                amplitude;
            transform.position = new Vector3(c.x, offset, c.z);
        }
    }
    
    void Start()
    {
        for (int i = 0; i < Zcount; i++) {
            for (int j = 0; j < Xcount; j++)
            {
                var cubeTransf = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                cubeTransf.position = new Vector3(Spacing * j, 0, Spacing * i);
                cubeTransf.parent = transform;
                AllCubes.Add(cubeTransf);
            }
        }
        AllCubesArray = AllCubes.ToArray();
        transformAccessArray = new TransformAccessArray(AllCubesArray, 4); //3 worker threads
    }

    void Update()
    {
        if (UseJobs)
        {
            using (ProfilerWithJobs.Auto())
            {
                // var nodeIndexes = new NativeArray<int>(AllCubes.Count, Allocator.TempJob); //Só 1 frame
                // var nodes = new NativeList<Vector3>(AllCubes.Count, Allocator.TempJob);
                var moveCubes = new MoveCubesJob()
                {
                    realtimeSinceStartup = Time.realtimeSinceStartup,
                    amplitude = amplitude,
                    period = period,
                    Speed = Speed
                };
                //var transforms = transformAccessArray;
                var jobHandle = moveCubes.Schedule(transformAccessArray);
                jobHandle.Complete();
                //transforms.Dispose();
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
                        Mathf.Sin(((Mathf.PI * 2) / period) * ((Time.realtimeSinceStartup * Speed) + (c.x + c.z))) *
                        amplitude;
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
