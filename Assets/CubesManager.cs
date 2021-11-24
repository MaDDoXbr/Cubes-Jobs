using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
//https://www.khanacademy.org/computing/computer-programming/programming-natural-simulations/programming-oscillations/a/oscillation-amplitude-and-period

public class CubesManager : MonoBehaviour
{
    public List<Transform> AllCubes;
    public int Xcount = 100, Zcount = 100;
    public float Spacing = 1.2f;
    public float Speed = 4f;

    public float amplitude = 2f, period = 4f;

    public struct MoveCubesJob : IJobParallelForTransform
    {
        public float deltaTime;
        public NativeArray<Vector3> Nodes;
        public NativeArray<int> enemyNodeIndexes;
        public NativeList<int> EnemiesToRemove;      
        public void Execute(int index, TransformAccess transform) //Versão do Transfor no Jobs System
        {

            //transform.position, etc
            transform.position =
                Vector3.MoveTowards(transform.position, Nodes[enemyNodeIndexes[index]], 5f * deltaTime);
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
    }
    
    void Update()
    {
        foreach (var cube in AllCubes)
        {
            var c = cube.position;
            var offset = Mathf.Sin(((Mathf.PI * 2) / period) * ((Time.realtimeSinceStartup * Speed) + (c.x + c.z))) * amplitude ;
            //var offset = amplitude * Mathf.Sin(2*Mathf.PI * frameCount / (period *  c.x + c.z));
            cube.position = new Vector3(c.x, offset, c.z);
        }
    }
}
