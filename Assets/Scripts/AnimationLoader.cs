using System.Collections;
using System.Collections.Generic;
using System.IO;
using ShotgunCalculator;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace ShotgunCalculator
{
    public class AnimationLoader : MonoBehaviour
    {
        public bool build;
        public TextAsset asset;
        [Range(.01f, 60)]
        public float framerate = 30;
        public int smoothing = 1;
        public Transform[] bones;
        public AnimationFrame[] frames;
        HitboxLoader hitboxLoader;
        float lastFrameTime;
        int currentFrame;

        [System.Serializable]
        public class BonePose
        {
            public int boneID;
            public Vector3 localPos;
            public Quaternion localRot;
        }

        [System.Serializable]
        public class AnimationFrame
        {
            public float time;
            public BonePose[] poses;
        }

        void Start()
        {
            build = true;
            hitboxLoader = gameObject.GetComponent<HitboxLoader>();
        }

        public void Build()
        {
            build = false;
            ParseBones();
            ParseFrames();

            ApplyFrame(frames[0]);
            currentFrame = 0;
            lastFrameTime = Time.time;
        }

        void Update()
        {
            if (build && hitboxLoader.ready)
                Build();

            if (frames != null && frames.Length > 0)
            {
                float currentTime = Time.time;
                float period = 1.0f / framerate;

                while (currentTime > lastFrameTime + period)
                {
                    lastFrameTime += period;
                    currentFrame++;
                }

                float delta = Time.time - lastFrameTime;

                currentFrame = frames.Length == 1 ? 0 : currentFrame % (frames.Length - 1);
                int nextFrame = currentFrame + 1;
                if (nextFrame >= frames.Length)
                    nextFrame -= frames.Length;

                float interpPercent = delta / period;
                ApplyFrame(frames[currentFrame], frames[nextFrame], interpPercent);

            }
        }

        void ParseBones()
        {
            string data = asset.text;
            using (StringReader reader = new StringReader(data))
            {
                string line;

                while ((line = reader.ReadLine()) != null && !line.TrimStart().StartsWith("nodes"))
                {
                    // Debug.Log("skipping:\t" + line);
                }

                List<Transform> boneList = new List<Transform>();
                while ((line = reader.ReadLine()) != null && !line.TrimStart().StartsWith("end"))
                {
                    string[] parts = line.Trim().Split(' ');
                    int boneID = Util.GetInt(parts, 0);
                    string boneName = Util.TrimName(parts[1]);
                    while (boneID >= boneList.Count)
                        boneList.Add(null);
                    boneList[boneID] = Util.RecursiveFind(transform, boneName);
                }
                bones = boneList.ToArray();
            }
        }

        void ParseFrames()
        {
            string data = asset.text;
            List<AnimationFrame> frameList = new List<AnimationFrame>();
            using (StringReader reader = new StringReader(data))
            {
                string line;

                while ((line = reader.ReadLine()) != null && !line.TrimStart().StartsWith("skeleton"))
                {
                    Debug.Log("skipping:\t" + line);
                }
                Debug.Log(line);
                reader.Peek();
                // line = reader.ReadLine();
                Debug.Log(line);
                while (reader.Peek() > 0)
                {
                    while (!line.TrimStart().StartsWith("time") && (line = reader.ReadLine()) != null)
                    {
                        Debug.Log("skipping:\t" + line);
                    }

                    AnimationFrame frame = new AnimationFrame();
                    frame.time = Util.GetFloat(line.Trim().Split(' '), 1);

                    List<BonePose> poses = new List<BonePose>();
                    int indentCount = Util.GetIndentCount(line);
                    while ((line = reader.ReadLine()) != null && Util.GetIndentCount(line) > indentCount)
                    {
                        string[] parts = line.Trim().Split(' ');
                        BonePose pose = new BonePose();
                        pose.boneID = Util.GetInt(parts, 0);
                        Vector3 pos = Util.GetVector3(parts, 1);
                        pos = new Vector3(pos.x, pos.y, -pos.z);
                        pose.localPos = pos;

                        Vector3 rot = Mathf.Rad2Deg * Util.GetVector3(parts, 4);
                        rot = new Vector3(-rot.x, -rot.y, rot.z);
                        pose.localRot = Util.XYZRotation(rot);

                        if (bones[pose.boneID].gameObject.name == "pelvis")
                        {
                            Quaternion offset = Quaternion.Euler(90, 0, 0);
                            pose.localRot = offset * pose.localRot;

                            pose.localPos.x = 0;
                            pose.localPos.y = 0;
                            pose.localPos = offset * pose.localPos;
                        }

                        poses.Add(pose);
                    }

                    frame.poses = poses.ToArray();
                    frameList.Add(frame);

                    if (line.TrimStart().StartsWith("end"))
                    {
                        Debug.Log("reached end:\t" + line);
                        Debug.Log("remaining: " + reader.ReadToEnd());
                    }
                }
            }
            frames = frameList.ToArray();
        }

        void ApplyFrame(AnimationFrame frameA, AnimationFrame frameB, float interp)
        {
            for (int i = 0; i < frameA.poses.Length; i++)
            {
                BonePose poseA = frameA.poses[i];
                BonePose poseB = frameB.poses[i];
                Transform bone = bones[poseA.boneID];
                if (bone != null)
                {
                    Quaternion targetRot = Quaternion.LerpUnclamped(poseA.localRot, poseB.localRot, interp);
                    Vector3 targetPos = Vector3.LerpUnclamped(poseA.localPos, poseB.localPos, interp);
                    float lerpPct = Util.GetLerpFract((smoothing / framerate) / framerate);
                    bone.localRotation = Quaternion.LerpUnclamped(bone.localRotation, targetRot, lerpPct);
                    bone.localPosition = Vector3.LerpUnclamped(bone.localPosition, targetPos, lerpPct);
                }
            }
        }

        void ApplyFrame(AnimationFrame frame)
        {
            ApplyFrame(frame, frame, 0);
        }
    }
}