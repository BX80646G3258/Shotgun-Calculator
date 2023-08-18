using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShotgunCalculator
{
    public static class Util
    {
        public static Vector3 Mul(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }

        public static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public static Mesh CopyMesh(Mesh mesh)
        {
            Mesh newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.triangles = mesh.triangles;
            newMesh.normals = mesh.normals;
            newMesh.tangents = mesh.tangents;
            newMesh.uv = mesh.uv;
            return newMesh;
        }

        public static string TrimName(string name)
        {
            return name.Substring(1, name.Length - 2);
        }

        public static float GetFloat(string[] parts, int index)
        {
            return float.Parse(parts[index], CultureInfo.InvariantCulture.NumberFormat);
        }

        public static int GetInt(string[] parts, int index)
        {
            return int.Parse(parts[index], CultureInfo.InvariantCulture.NumberFormat);
        }

        public static Transform RecursiveFind(Transform transform, string name)
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

        public static Vector3 GetVector3(string[] parts, int index)
        {
            return new Vector3(GetFloat(parts, index), GetFloat(parts, index + 1), GetFloat(parts, index + 2));
        }

        public static void RecursiveDraw(Transform transform, Color color1, Color color2)
        {
            Debug.DrawRay(transform.position, .01f * transform.up, color2);
            foreach (Transform child in transform)
            {
                Debug.DrawLine(transform.position, child.position, color1);
                RecursiveDraw(child, color1, color2);
            }
        }

        public static string ArrayToString(string[] parts)
        {
            string output = "";
            foreach (string part in parts)
            {
                output += "\t\"" + part + "\"";
            }
            return output;
        }

        public static int GetIndentCount(string line)
        {
            for (int i = 0; i < line.Length; i++)
                if (line[i] != ' ')
                    return i;

            // the entire line was spaces
            return -1;
        }

        public static Quaternion XYZRotation(Vector3 rotation)
        {
            Quaternion output = Quaternion.identity;
            output *= Quaternion.AngleAxis(rotation.z, Vector3.forward);
            output *= Quaternion.AngleAxis(rotation.y, Vector3.up);
            output *= Quaternion.AngleAxis(rotation.x, Vector3.right);
            return output;
        }

        public static Vector3 SourceToUnityPos(Vector3 v)
        {
            return new Vector3(v.x, v.y, -v.z);
        }

        public static Vector3 SourceToUnityRot(Vector3 r)
        {
            return new Vector3(-r.z, -r.x, r.y);
        }

        public static float GetLerpFract(float smoothing, float percent = .99f)
        {
            return 1 - Mathf.Exp((Mathf.Log(1f - percent) / smoothing) * Time.deltaTime);
        }
    }
}