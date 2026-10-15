using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace RobotML.Candidates
{
    public enum CandidateContract { Bipedal175, Bipedal158, Bipedal191, Quad126, Quad113, Forced113 }

    // Isolated inference adapter: preserves the historical tensor order without
    // swapping script GUIDs or changing the original training prefabs.
    public sealed class CandidateRobotAgent : Unity.MLAgents.Agent
    {
        public CandidateContract contract;
        public ArticulationBody rootBody;
        public ArticulationBody[] joints;
        public float[] maximumJointForces;
        public Transform[] feet;
        public Transform target;
        [Min(0.05f)] public float targetArrivalDistance = 0.75f;
        [Min(1f)] public float targetRoamRadius = 6f;
        [Min(0.1f)] public float minimumNextTargetDistance = 2f;
        public int targetsReached;
        public int[] legOrder = { 3, 0, 1, 2 };
        public float availableForce = 6050;
        public float targetSpeed = 2;
        public float targetHeight = 3;
        public string modelPath;
        public string sourceCommit;
        public bool reconstructedPhysics;
        public int observationCount;
        public int actionCount;
        public int receivedActions;
        public int completedEpisodes;
        public float observedSpeed;
        public float speedError;
        public bool outputsFinite = true;
        public int lastObservationCount;
        [Min(1f)] public float episodeTravelRadius = 7f;
        public bool randomizeEpisodeHeading;

        readonly List<float> initialPositions = new();
        readonly List<float> initialVelocities = new();
        ArticulationDrive[] initialDrives;
        Vector3 initialRootPosition;
        Quaternion initialRootRotation;
        Vector3 direction = Vector3.forward;
        Quaternion frameRotation = Quaternion.identity;
        float[] softShares;
        float outputForce;
        int firstLeg;
        bool initialized;

        public override void Initialize()
        {
            if (rootBody == null || target == null || joints == null ||
                maximumJointForces == null || joints.Length != maximumJointForces.Length)
                throw new InvalidOperationException("Candidate robot references are incomplete: " + name);
            foreach (var joint in joints)
                if (joint == null || joint.jointType != ArticulationJointType.RevoluteJoint)
                    throw new InvalidOperationException("Candidate needs one revolute DOF per joint: " + name);
            initialRootPosition = rootBody.transform.position;
            initialRootRotation = rootBody.transform.rotation;
            rootBody.GetJointPositions(initialPositions);
            rootBody.GetJointVelocities(initialVelocities);
            softShares = new float[joints.Length];
            initialDrives = new ArticulationDrive[joints.Length];
            for (int i = 0; i < joints.Length; i++) initialDrives[i] = joints[i].xDrive;
            initialized = true;
        }

        public override void OnEpisodeBegin()
        {
            if (!initialized) return;
            var rotation = randomizeEpisodeHeading ?
                Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0) * initialRootRotation : initialRootRotation;
            rootBody.TeleportRoot(initialRootPosition, rotation);
            rootBody.SetJointPositions(initialPositions);
            rootBody.SetJointVelocities(initialVelocities);
            rootBody.linearVelocity = Vector3.zero;
            rootBody.angularVelocity = Vector3.zero;
            for (int i = 0; i < joints.Length; i++)
            {
                var joint = joints[i];
                joint.linearVelocity = Vector3.zero;
                joint.angularVelocity = Vector3.zero;
                var drive = initialDrives[i];
                drive.forceLimit = 0;
                joint.xDrive = drive;
            }
            outputForce = 0;
            Array.Clear(softShares, 0, softShares.Length);
            direction = Vector3.forward;
            firstLeg = 0;
            completedEpisodes++;
        }

        Quaternion RotationDelta()
        {
            var relative = rootBody.transform.InverseTransformDirection(direction);
            return relative.sqrMagnitude < 0.00001f ? Quaternion.identity : Quaternion.LookRotation(relative);
        }

        Vector3 SignedRotationDelta()
        {
            var angles = RotationDelta().eulerAngles;
            return new Vector3(angles.x < 180 ? angles.x : angles.x - 360,
                angles.y < 180 ? angles.y : angles.y - 360,
                angles.z < 180 ? angles.z : angles.z - 360);
        }

        void UpdateFrame()
        {
            // Historical symmetry code sorts before refreshing its target frame.
            var yaw = SignedRotationDelta().y;
            firstLeg = Mathf.Abs(yaw) < 45 ? 0 : Mathf.Abs(yaw) < 135 ? (yaw > 0 ? 1 : 3) : 2;
            var delta = target.position - rootBody.transform.position;
            delta.y = 0;
            direction = delta.sqrMagnitude > 0.00001f ? delta.normalized : Vector3.forward;
            frameRotation = Quaternion.LookRotation(direction);
        }

        Vector3 InFrame(Vector3 value) => Quaternion.Inverse(frameRotation) * value;
        float JointPosition(int i)
        {
            var drive = joints[i].xDrive;
            return Mathf.InverseLerp(drive.lowerLimit, drive.upperLimit,
                joints[i].jointPosition[0] * Mathf.Rad2Deg);
        }
        float JointTarget(int i)
        {
            var drive = joints[i].xDrive;
            return Mathf.InverseLerp(drive.lowerLimit, drive.upperLimit, drive.target);
        }
        float ForceRatio(int i) => Mathf.InverseLerp(0, maximumJointForces[i], joints[i].xDrive.forceLimit);

        Vector3 AverageVelocity(bool includeRoot)
        {
            var sum = includeRoot ? rootBody.linearVelocity : Vector3.zero;
            int count = includeRoot ? 1 : 0;
            foreach (var joint in joints)
                if (includeRoot || joint.mass > 0) { sum += joint.linearVelocity; count++; }
            return count == 0 ? Vector3.zero : sum / count;
        }

        static void Add(List<float> list, Vector3 value)
        { list.Add(value.x); list.Add(value.y); list.Add(value.z); }
        int SortedLeg(int index) => legOrder[(firstLeg + index) % 4];

        // Also used by the Editor's structural and Play Mode checks.
        public List<float> BuildObservations()
        {
            UpdateFrame();
            var values = new List<float>(observationCount);
            bool bipedal = contract <= CandidateContract.Bipedal191;
            var average = AverageVelocity(!bipedal);
            var goal = targetSpeed * direction;
            observedSpeed = average.magnitude;
            speedError = Vector3.Distance(average, goal);
            if (bipedal)
            {
                for (int i = 0; i < joints.Length; i++)
                {
                    if (contract == CandidateContract.Bipedal158) values.Add(softShares == null ? 0 : softShares[i]);
                    else
                    {
                        values.Add(maximumJointForces[i]); values.Add(ForceRatio(i));
                        values.Add(JointTarget(i) - JointPosition(i));
                    }
                    values.Add(JointTarget(i)); values.Add(JointPosition(i));
                    values.Add(joints[i].jointVelocity[0]); values.Add(joints[i].jointAcceleration[0]);
                }
                if (contract == CandidateContract.Bipedal158)
                { values.Add(availableForce); values.Add(outputForce / Mathf.Max(availableForce, 0.001f)); values.Add(targetHeight); }
                values.Add(speedError);
                values.Add(Vector3.Dot(direction, rootBody.transform.forward));
                values.Add(Vector3.Dot(rootBody.transform.up, Vector3.up));
                foreach (var joint in joints)
                {
                    Add(values, InFrame(joint.transform.position - rootBody.transform.position));
                    Add(values, InFrame(joint.angularVelocity)); Add(values, InFrame(joint.linearVelocity));
                }
                Add(values, InFrame(rootBody.angularVelocity)); Add(values, InFrame(rootBody.linearVelocity));
                Add(values, InFrame(average)); Add(values, InFrame(goal));
            }
            else if (contract == CandidateContract.Quad126)
            {
                for (int i = 0; i < joints.Length; i++) AddDrive(values, i);
                values.Add(speedError); Add(values, InFrame(average)); Add(values, InFrame(goal));
                var relative = rootBody.transform.InverseTransformPoint(target.position);
                Add(values, relative);
                var rotation = relative.sqrMagnitude > 0.00001f ? Quaternion.LookRotation(relative) : Quaternion.identity;
                values.Add(rotation.x); values.Add(rotation.y); values.Add(rotation.z); values.Add(rotation.w);
            }
            else
            {
                for (int l = 0; l < 4; l++)
                {
                    int leg = SortedLeg(l);
                    for (int j = 0; j < 3; j++) AddDrive(values, leg * 3 + j);
                    Add(values, InFrame(feet[leg].position - rootBody.transform.position));
                }
                values.Add(Physics.Raycast(rootBody.transform.position, Vector3.down, out var hit, 1) ? hit.distance : 1);
                values.Add(speedError); Add(values, InFrame(average)); Add(values, InFrame(goal));
                Add(values, SignedRotationDelta() / 180);
                Add(values, InFrame(rootBody.linearVelocity)); Add(values, InFrame(rootBody.angularVelocity));
            }
            lastObservationCount = values.Count;
            return values;
        }

        void AddDrive(List<float> values, int index)
        {
            values.Add(maximumJointForces[index]); values.Add(ForceRatio(index));
            values.Add(JointTarget(index)); values.Add(JointTarget(index) - JointPosition(index));
            values.Add(JointPosition(index)); values.Add(joints[index].jointVelocity[0]);
            values.Add(joints[index].jointAcceleration[0]);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            var observations = BuildObservations();
            if (observations.Count != observationCount)
                throw new InvalidOperationException($"{name}: {observations.Count} observations, expected {observationCount}");
            foreach (float value in observations)
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    throw new InvalidOperationException("Non-finite observation: " + name);
                sensor.AddObservation(value);
            }
        }

        public override void OnActionReceived(ActionBuffers buffers)
        {
            var actions = buffers.ContinuousActions;
            if (actions.Length != actionCount) throw new InvalidOperationException("Action count mismatch: " + name);
            for (int i = 0; i < actions.Length; i++)
                if (float.IsNaN(actions[i]) || float.IsInfinity(actions[i]))
                { outputsFinite = false; throw new InvalidOperationException("Non-finite action: " + name); }
            float Unit(int i) => Mathf.Clamp01((actions[i] + 1) * 0.5f);
            var ratios = new float[joints.Length];
            var targets = new float[joints.Length];
            if (contract == CandidateContract.Forced113)
            {
                for (int i = 0; i < 12; i++) ratios[i] = Unit(i);
                for (int pair = 0; pair < 3; pair++)
                {
                    float center = Unit(12 + pair * 2), offset = Unit(13 + pair * 2) * 2 - 1;
                    float range = Mathf.Min(center, 1 - center);
                    if (pair == 0)
                    {
                        targets[SortedLeg(0) * 3] = center + range * offset;
                        targets[SortedLeg(1) * 3] = 1 - center + range * offset;
                        targets[SortedLeg(2) * 3] = center - range * offset;
                        targets[SortedLeg(3) * 3] = 1 - center - range * offset;
                    }
                    else for (int l = 0; l < 4; l++)
                        targets[SortedLeg(l) * 3 + pair] = center +
                            (pair == 1 ? (l % 2 == 0 ? 1 : -1) : (l % 2 == 0 ? -1 : 1)) * range * offset;
                }
                // Original Forced code rotates force allocation together with leg order.
                var sortedRatios = new float[12];
                for (int l = 0; l < 4; l++) for (int j = 0; j < 3; j++)
                    sortedRatios[SortedLeg(l) * 3 + j] = ratios[l * 3 + j];
                ApplyBudget(ratios, targets, sortedRatios);
            }
            else
            {
                int offset = contract == CandidateContract.Bipedal158 ? 1 : 0;
                for (int i = 0; i < joints.Length; i++) { ratios[i] = Unit(offset + i * 2); targets[i] = Unit(offset + i * 2 + 1); }
                if (contract == CandidateContract.Bipedal158)
                {
                    outputForce = availableForce * Unit(0);
                    float sum = 0; foreach (float ratio in ratios) sum += ratio;
                    for (int i = 0; i < joints.Length; i++)
                    {
                        softShares[i] = sum > 0 ? ratios[i] / sum : 0;
                        SetDrive(i, targets[i], outputForce * softShares[i]);
                    }
                }
                else if (contract == CandidateContract.Quad113)
                {
                    var sortedTargets = new float[joints.Length];
                    var sortedRatios = new float[joints.Length];
                    for (int l = 0; l < 4; l++) for (int j = 0; j < 3; j++)
                    { sortedTargets[SortedLeg(l) * 3 + j] = targets[l * 3 + j]; sortedRatios[SortedLeg(l) * 3 + j] = ratios[l * 3 + j]; }
                    ApplyBudget(ratios, sortedTargets, sortedRatios);
                }
                else ApplyBudget(ratios, targets, ratios);
            }
            receivedActions++;
        }

        void ApplyBudget(float[] requestedRatios, float[] targets, float[] appliedRatios)
        {
            float requested = 0;
            for (int i = 0; i < joints.Length; i++) requested += maximumJointForces[i] * requestedRatios[i];
            float scale = requested > availableForce ? availableForce / requested : 1;
            for (int i = 0; i < joints.Length; i++) SetDrive(i, targets[i], maximumJointForces[i] * appliedRatios[i] * scale);
        }

        void SetDrive(int index, float targetRatio, float force)
        {
            var drive = joints[index].xDrive;
            drive.target = Mathf.Lerp(drive.lowerLimit, drive.upperLimit, targetRatio);
            drive.forceLimit = force;
            joints[index].xDrive = drive;
        }

        void FixedUpdate()
        {
            if (!initialized) return;
            var offset = rootBody.transform.position - initialRootPosition;
            if (offset.y < -1.5f || new Vector2(offset.x, offset.z).magnitude > episodeTravelRadius ||
                (StepCount > 50 && Vector3.Dot(rootBody.transform.up, Vector3.up) < 0.15f))
            {
                EndEpisode();
                return;
            }
            TryAdvanceTarget();
        }

        public bool TryAdvanceTarget()
        {
            if (rootBody == null || target == null) return false;
            var delta = target.position - rootBody.transform.position;
            delta.y = 0;
            if (delta.sqrMagnitude > targetArrivalDistance * targetArrivalDistance) return false;

            // The target is a child of its own floor tile. Sample in tile coordinates
            // so other candidates' targets never move into this robot's arena.
            var tile = target.parent;
            var previous = target.localPosition;
            var radius = Mathf.Clamp(targetRoamRadius, 1f, 6f);
            var separation = Mathf.Clamp(minimumNextTargetDistance, targetArrivalDistance + 0.1f, radius);
            var next = previous;
            bool found = false;
            for (int attempt = 0; attempt < 64; attempt++)
            {
                var point = UnityEngine.Random.insideUnitCircle * radius;
                next = new Vector3(point.x, previous.y, point.y);
                var world = tile != null ? tile.TransformPoint(next) : next;
                var fromRobot = world - rootBody.transform.position;
                fromRobot.y = 0;
                if (fromRobot.sqrMagnitude >= separation * separation)
                { found = true; break; }
            }
            if (!found)
            {
                var localRobot = tile != null ? tile.InverseTransformPoint(rootBody.transform.position) : rootBody.transform.position;
                var away = new Vector2(-localRobot.x, -localRobot.z);
                if (away.sqrMagnitude < 0.00001f) away = Vector2.right;
                away = away.normalized * radius;
                next = new Vector3(away.x, previous.y, away.y);
            }
            target.localPosition = next;
            targetsReached++;
            return true;
        }
    }
}
