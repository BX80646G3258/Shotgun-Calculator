using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShotgunCalculator
{
    public class HitboxLoader : MonoBehaviour
    {
        // public string path;
        public TextAsset asset;
        public bool rebuild;
        public bool build;
        public bool ready;
        public Mesh capsuleMesh;
        public Mesh boxMesh;
        public int negVariant = 0;
        public Material[] materials;

        void Start()
        {
            rebuild = true;
            build = false;
            ready = false;
        }

        void Update()
        {
            if (build)
            {
                build = false;
                ready = false;
                Build();
                negVariant++;
            }

            if (rebuild)
            {
                rebuild = false;
                ready = false;
                foreach (Transform child in transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                build = true;
            }
            Util.RecursiveDraw(transform, Color.red, Color.green);
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
                        string boneName = Util.TrimName(parts[1]);
                        string boneParent = Util.TrimName(parts[2]);
                        Vector3 position = Util.GetVector3(parts, 3);
                        Vector3 rotation = Util.GetVector3(parts, 6);
                        GameObject bone = new GameObject(boneName);
                        // bone.transform.parent = transform;
                        bone.transform.SetParent(transform, false);
                        if (boneParent.Length > 0)
                            bone.transform.SetParent(Util.RecursiveFind(transform, boneParent));
                        // bone.transform.localScale = Vector3.one;
                        // bone.transform.localPosition = new Vector3(position.x, position.y, -position.z);
                        bone.transform.localPosition = Util.SourceToUnityPos(position);

                        // rotation = new Vector3(-rotation.z, -rotation.x, rotation.y);
                        rotation = Util.SourceToUnityRot(rotation);
                        // rotation = new Vector3(rotation.z, rotation.x, rotation.y);
                        // for (int i = 0; i < 3; i++)
                        // {
                        //     if ((negVariant & 1 << i) > 0)
                        //         rotation[i] = -rotation[i];
                        // }

                        bone.transform.localRotation = Util.XYZRotation(rotation);
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
                        int groupNumber = Util.GetInt(parts, 1);
                        string parent = Util.TrimName(parts[2]);
                        Vector3 p1 = Util.GetVector3(parts, 3);
                        Vector3 p2 = Util.GetVector3(parts, 6);
                        Vector3 rotation = Util.GetVector3(parts, 9);
                        float radius = Util.GetFloat(parts, 12);
                        GameObject hitbox = new GameObject(parent + " hbox");
                        // hitbox.transform.parent = transform;
                        hitbox.transform.SetParent(Util.RecursiveFind(transform, parent), false);

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

                            collider.size = Util.Abs(direction);
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

            ready = true;
        }

        void AddColliderMeshSibling(Collider collider, Material mat)
        {
            GameObject colliderGameObject = collider.gameObject;
            GameObject meshGameObject = new GameObject(colliderGameObject.name + " mesh");
            // meshGameObject.transform.parent = colliderGameObject.transform.parent;
            meshGameObject.transform.SetParent(colliderGameObject.transform.parent, false);
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
                mesh = Util.CopyMesh(capsuleMesh);
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
                mesh = Util.CopyMesh(boxMesh);
                vertices = mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 v = vertices[i];
                    v = Util.Mul(v, box.size) + box.center;
                    vertices[i] = v;
                }

            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            filter.mesh = mesh;
        }

    }
}