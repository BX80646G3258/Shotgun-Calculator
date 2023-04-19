using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class HitboxLoader : MonoBehaviour
{
    // public string path;
    public TextAsset asset;
    public bool rebuild;
    public bool build;
    public Mesh capsuleMesh;
    public Mesh boxMesh;
    public int negVariant = 0;
    public Material[] materials;

    void Start()
    {
        rebuild = true;
        build = false;
    }

    void Update()
    {
        if (build)
        {
            build = false;
            Build();
            negVariant++;
        }

        if (rebuild)
        {
            rebuild = false;
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            build = true;
        }
        RecursiveDraw(transform, Color.red, Color.green);
    }

    void Build()
    {
        string data = asset.text;
        using (StringReader reader = new StringReader(data))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(' ');
                if (parts[0] == "$definebone")
                {
                    string boneName = TrimName(parts[1]);
                    string boneParent = TrimName(parts[2]);
                    Vector3 position = GetVector3(parts, 3);
                    Vector3 rotation = GetVector3(parts, 6);
                    GameObject bone = new GameObject(boneName);
                    bone.transform.parent = transform;
                    if (boneParent.Length > 0)
                        bone.transform.SetParent(RecursiveFind(transform, boneParent));
                    bone.transform.localPosition = new Vector3(position.x, position.y, -position.z);

                    rotation = new Vector3(-rotation.z, -rotation.x, rotation.y);
                    // rotation = new Vector3(rotation.z, rotation.x, rotation.y);
                    // for (int i = 0; i < 3; i++)
                    // {
                    //     if ((negVariant & 1 << i) > 0)
                    //         rotation[i] = -rotation[i];
                    // }

                    Quaternion orderedRotation = Quaternion.identity;
                    orderedRotation *= Quaternion.AngleAxis(rotation.z, Vector3.forward);
                    orderedRotation *= Quaternion.AngleAxis(rotation.y, Vector3.up);
                    orderedRotation *= Quaternion.AngleAxis(rotation.x, Vector3.right);

                    bone.transform.localRotation = orderedRotation;
                }
            }
        }

        using (StringReader reader = new StringReader(data))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(' ');
                if (parts[0] == "$hbox")
                {
                    int groupNumber = GetInt(parts, 1);
                    string parent = TrimName(parts[2]);
                    Vector3 p1 = GetVector3(parts, 3);
                    Vector3 p2 = GetVector3(parts, 6);
                    Vector3 rotation = GetVector3(parts, 9);
                    float radius = GetFloat(parts, 12);
                    GameObject hitbox = new GameObject(parent + " hbox");
                    hitbox.transform.parent = transform;
                    hitbox.transform.SetParent(RecursiveFind(transform, parent));

                    p1 = new Vector3(p1.x, p1.y, -p1.z);
                    p2 = new Vector3(p2.x, p2.y, -p2.z);

                    Vector3 center = (p1 + p2) / 2;
                    hitbox.transform.localPosition = center;
                    Vector3 direction = p1 - p2;
                    if (radius > 0)
                    {
                        CapsuleCollider collider = hitbox.AddComponent<CapsuleCollider>();
                        float length = direction.magnitude;
                        collider.radius = radius;
                        collider.height = length + radius * 2;
                        Vector3 boneForward = direction.x == 0 ? Vector3.right : Vector3.forward;
                        hitbox.transform.localRotation = Quaternion.AngleAxis(90, Vector3.up) * Quaternion.LookRotation(direction, boneForward);

                        AddColliderMeshSibling(collider, materials[groupNumber - 1]);
                    }
                    else
                    {
                        BoxCollider collider = hitbox.AddComponent<BoxCollider>();
                        
                        collider.size = Abs(direction);
                        rotation = new Vector3(-rotation.z, -rotation.x, rotation.y);
                        Quaternion orderedRotation = Quaternion.identity;
                        orderedRotation *= Quaternion.AngleAxis(rotation.z, Vector3.forward);
                        orderedRotation *= Quaternion.AngleAxis(rotation.y, Vector3.up);
                        orderedRotation *= Quaternion.AngleAxis(rotation.x, Vector3.right);

                        hitbox.transform.localRotation = orderedRotation;
                        AddColliderMeshSibling(collider, materials[groupNumber - 1]);
                    }
                }
            }
        }
    }

    void AddColliderMeshSibling(Collider collider, Material mat)
    {
        GameObject colliderGameObject = collider.gameObject;
        GameObject meshGameObject = new GameObject(colliderGameObject.name + " mesh");
        meshGameObject.transform.parent = colliderGameObject.transform.parent;
        meshGameObject.transform.localPosition = colliderGameObject.transform.localPosition;
        meshGameObject.transform.localRotation = colliderGameObject.transform.localRotation;
        MeshFilter filter = meshGameObject.gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = meshGameObject.gameObject.AddComponent<MeshRenderer>();
        renderer.material = mat;

        Mesh mesh = null;
        Vector3[] vertices = null;

        if (collider is CapsuleCollider)
        {
            CapsuleCollider capsule = collider as CapsuleCollider;
            mesh = CopyMesh(capsuleMesh);
            vertices = mesh.vertices;
            float heightOffset = -capsule.radius * 2 + capsule.height / 2;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                v = v * capsule.radius * 2;
                v.y += v.y > 0 ? heightOffset : -heightOffset;
                vertices[i] = v;
            }
        }
        else if (collider is BoxCollider)
        {
            BoxCollider box = collider as BoxCollider;
            mesh = CopyMesh(boxMesh);
            vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                v = Mul(v, box.size) + box.center;
                vertices[i] = v;
            }

        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        filter.mesh = mesh;
    }

    Vector3 Mul(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    Mesh CopyMesh(Mesh mesh)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.normals = mesh.normals;
        newMesh.tangents = mesh.tangents;
        newMesh.uv = mesh.uv;
        return newMesh;
    }

    string TrimName(string name)
    {
        return name.Substring(1, name.Length - 2);
    }

    float GetFloat(string[] parts, int index)
    {
        return float.Parse(parts[index], CultureInfo.InvariantCulture.NumberFormat);
    }

    int GetInt(string[] parts, int index)
    {
        return int.Parse(parts[index], CultureInfo.InvariantCulture.NumberFormat);
    }

    Transform RecursiveFind(Transform transform, string name)
    {
        foreach (Transform child in transform)
        {
            if (child.name == name)
                return child;
            Transform found = RecursiveFind(child, name);
            if (found)
                return found;
        }
        return null;
    }

    Vector3 GetVector3(string[] parts, int index)
    {
        return new Vector3(GetFloat(parts, index), GetFloat(parts, index + 1), GetFloat(parts, index + 2));
    }

    void RecursiveDraw(Transform transform, Color color1, Color color2)
    {
        Debug.DrawRay(transform.position, transform.up, color2);
        foreach (Transform child in transform)
        {
            Debug.DrawLine(transform.position, child.position, color1);
            RecursiveDraw(child, color1, color2);
        }
    }
}
