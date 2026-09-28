using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SkillRangeContainer
{
    public RangeType type;
    public float radius;
    public List<Vector2> vertexList;
}

public class SkillRange : MonoBehaviour
{
    private const int CIRCLE_SEGMENT_COUNT = 64;
    private const int VERTICES_PER_TRIANGLE = 3;
    
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private LineRenderer _lineRenderer;
    
    public void Init(SkillRangeContainer argContainer)
    {
        if (argContainer.type == RangeType.Circle)
        {
            SetCircle(argContainer.radius);
        }
        else
        {
            SetPolygon(argContainer.vertexList);
        }
    }

    void SetCircle(float argRadius)
    {
        Vector3[] vertices = new Vector3[CIRCLE_SEGMENT_COUNT + 1];

        // 중심점
        vertices[0] = Vector3.zero;

        // 원 둘레
        for (int i = 0; i < CIRCLE_SEGMENT_COUNT; i++)
        {
            float angle = i * Mathf.PI * 2f / CIRCLE_SEGMENT_COUNT;

            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * argRadius,
                Mathf.Sin(angle) * argRadius,
                0f
            );
        }

        CreateCircleMesh(vertices);
        SetCircleBorder(vertices);
    }

    void CreateCircleMesh(Vector3[] argVertices)
    {
        Mesh mesh = new Mesh();
        
        int[] triangles = new int[CIRCLE_SEGMENT_COUNT * VERTICES_PER_TRIANGLE];

        // 삼각형 그룹 연결
        for (int i = 0; i < CIRCLE_SEGMENT_COUNT; i++)
        {
            int triangleIndex = i * VERTICES_PER_TRIANGLE;

            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = ((i + 1) % CIRCLE_SEGMENT_COUNT) + 1;
        }

        mesh.name = "CircleRange";
        mesh.vertices = argVertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        _meshFilter.mesh = mesh;
    }
    
    void SetCircleBorder(Vector3[] argVertices)
    {
        _lineRenderer.positionCount = CIRCLE_SEGMENT_COUNT;
        _lineRenderer.loop = true;

        for (int i = 0; i < CIRCLE_SEGMENT_COUNT; i++)
        {
            _lineRenderer.SetPosition(i, argVertices[i + 1]);
        }
    }

    void SetPolygon(List<Vector2> argVertexList)
    {
        var mesh = new Mesh();

        var vertices = new Vector3[argVertexList.Count];

        for (int i = 0; i < argVertexList.Count; i++)
        {
            vertices[i] = argVertexList[i];
        }

        var triangles = Trianglate(argVertexList);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        _meshFilter.mesh = mesh;

        SetPolygonBorder(argVertexList);
    }
    
    int[] Trianglate(List<Vector2> argVertexList)
    {
        var indices = new List<int>();

        for (int i = 0; i < argVertexList.Count; i++)
        {
            indices.Add(i);
        }

        var triangles = new List<int>();

        while (indices.Count > VERTICES_PER_TRIANGLE)
        {
            bool earFound = false;

            for (int i = 0; i < indices.Count; i++)
            {
                int prevIndex = indices[(i - 1 + indices.Count) % indices.Count];
                int currentIndex = indices[i];
                int nextIndex = indices[(i + 1) % indices.Count];

                Vector2 prev = argVertexList[prevIndex];
                Vector2 current = argVertexList[currentIndex];
                Vector2 next = argVertexList[nextIndex];

                if (!IsConvex(prev, current, next))
                {
                    continue;
                }

                if (ContainsVertex(argVertexList, indices, prevIndex, currentIndex, nextIndex))
                {
                    continue;
                }

                triangles.Add(prevIndex);
                triangles.Add(currentIndex);
                triangles.Add(nextIndex);

                indices.RemoveAt(i);

                earFound = true;
                break;
            }

            // 무한 루프 방지
            if (!earFound)
            {
                Debug.LogError("Failed to triangulate polygon.");
                return null;
            }
        }

        triangles.Add(indices[0]);
        triangles.Add(indices[1]);
        triangles.Add(indices[2]);

        return triangles.ToArray();
    }

    // 볼록한지 오목한지(즉 파여있어서 제대로 된 삼각형이 아닐 가능성)
    bool IsConvex(Vector2 argPrev, Vector2 argCurrent, Vector2 argNext)
    {
        Vector2 directionA = argCurrent - argPrev;
        Vector2 directionB = argNext - argCurrent;

        return Cross(directionA, directionB) > 0f;
    }

    bool ContainsVertex(List<Vector2> argVertexList, List<int> argIndices, int argPrev, int argCur, int argNext)
    {
        Vector2 a = argVertexList[argPrev];
        Vector2 b = argVertexList[argCur];
        Vector2 c = argVertexList[argNext];

        for (int i = 0; i < argIndices.Count; i++)
        {
            int index = argIndices[i];

            if (index == argPrev || index == argCur || index == argNext)
            {
                continue;
            }

            if (IsPointInsideTriangle(argVertexList[index], a, b, c))
            {
                return true;
            }
        }

        return false;
    }

    // 두 벡터를 외적하여 모두 양수면, 즉 모두 왼쪽에 point가 있다면 삼각형 내부라는 뜻
    bool IsPointInsideTriangle(Vector2 argPoint, Vector2 argA, Vector2 argB, Vector2 argC)
    {
        float crossAB = Cross(argB - argA, argPoint - argA);
        float crossBC = Cross(argC - argB, argPoint - argB);
        float crossCA = Cross(argA - argC, argPoint - argC);

        return crossAB >= 0f && crossBC >= 0f && crossCA >= 0f;
    }

    float Cross(Vector2 argA, Vector2 argB)
    {
        return argA.x * argB.y - argA.y * argB.x;
    }

    void SetPolygonBorder(List<Vector2> argVertexList)
    {
        _lineRenderer.positionCount = argVertexList.Count;
        _lineRenderer.loop = true;

        for (int i = 0; i < argVertexList.Count; i++)
        {
            _lineRenderer.SetPosition(i, argVertexList[i]);
        }
    }
    
    public void Destroy(Action<SkillRange> argCallback)
    {
        StartCoroutine(CoDestroy(argCallback));
    }

    // test code
    IEnumerator CoDestroy(Action<SkillRange> argCallback)
    {
        yield return new WaitForSeconds(5f);
        
        argCallback?.Invoke(this);
    }
}