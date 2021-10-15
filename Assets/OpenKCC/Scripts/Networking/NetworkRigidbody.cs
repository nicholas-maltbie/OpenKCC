using System;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkRigidbody : NetworkBehaviour
{
    public NetworkVariableVector3 netVelocity;
    public NetworkVariableVector3 netAngularVelocity;
    public NetworkVariableVector3 netPosition;
    public NetworkVariableQuaternion netRotation;
    public NetworkVariableUInt netUpdateId = new NetworkVariableUInt(new NetworkVariableSettings() { WritePermission = NetworkVariablePermission.OwnerOnly });

    [SerializeField]
    bool m_SyncVelocity = true;

    [SerializeField]
    bool m_SyncAngularVelocity = true;

    [SerializeField]
    bool m_SyncPosition = true;

    [SerializeField]
    bool m_SyncRotation = true;

    [SerializeField]
    float m_InterpolationTime;

    [SerializeField]
    float m_SyncRate = 20;

    NetworkVariablePermission permissionType = NetworkVariablePermission.OwnerOnly;

    [Serializable]
    struct InterpolationState
    {
        public Vector3 PositionDelta;
        public Quaternion RotationDelta;
        public Vector3 VelocityDelta;
        public Vector3 AngularVelocityDelta;
        public float TimeRemaining;
        public float TotalTime;
    }

    uint m_InterpolationChangeId;
    InterpolationState m_InterpolationState;
    Rigidbody m_Rigidbody;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        netVelocity = new NetworkVariableVector3(new NetworkVariableSettings()
        { WritePermission = permissionType, SendTickrate = m_SyncRate });
        netAngularVelocity = new NetworkVariableVector3(new NetworkVariableSettings()
        { WritePermission = permissionType, SendTickrate = m_SyncRate });
        netPosition = new NetworkVariableVector3(new NetworkVariableSettings()
        { WritePermission = permissionType, SendTickrate = m_SyncRate });
        netRotation = new NetworkVariableQuaternion(new NetworkVariableSettings()
        { WritePermission = permissionType, SendTickrate = m_SyncRate });
    }

    public void BeginInterpolation()
    {
        m_InterpolationState = new InterpolationState()
        {
            PositionDelta = netPosition.Value - m_Rigidbody.position,
            RotationDelta = Quaternion.Inverse(m_Rigidbody.rotation) * netRotation.Value,
            VelocityDelta = netVelocity.Value - m_Rigidbody.velocity,
            AngularVelocityDelta = netAngularVelocity.Value - m_Rigidbody.angularVelocity,
            TimeRemaining = m_InterpolationTime,
            TotalTime = m_InterpolationTime
        };
    }

    void FixedUpdate()
    {
        if (!NetworkVariablesInitialized())
        {
            return;
        }

        if (IsOwner)
        {
            bool changed = false;

            if (m_SyncPosition)
            {
                changed |= TryUpdate(netPosition, m_Rigidbody.position);
            }

            if (m_SyncRotation)
            {
                changed |= TryUpdate(netRotation, m_Rigidbody.rotation);
            }

            if (m_SyncVelocity)
            {
                changed |= TryUpdate(netVelocity, m_Rigidbody.velocity);
            }

            if (m_SyncAngularVelocity)
            {
                changed |= TryUpdate(netAngularVelocity, m_Rigidbody.angularVelocity);
            }

            if (changed)
            {
                netUpdateId.Value++;
            }
        }
        else
        {
            if (m_InterpolationChangeId != netUpdateId.Value)
            {
                BeginInterpolation();
                m_InterpolationChangeId = netUpdateId.Value;
            }

            float deltaTime = Time.fixedDeltaTime;

            if (0 < m_InterpolationState.TimeRemaining)
            {
                deltaTime = Mathf.Min(deltaTime, m_InterpolationState.TimeRemaining);
                m_InterpolationState.TimeRemaining -= deltaTime;

                deltaTime /= m_InterpolationState.TotalTime;

                if (m_SyncPosition)
                {
                    m_Rigidbody.position +=
                        m_InterpolationState.PositionDelta * deltaTime;
                }

                if (m_SyncRotation)
                {
                    m_Rigidbody.rotation =
                        m_Rigidbody.rotation * Quaternion.Slerp(Quaternion.identity, m_InterpolationState.RotationDelta, deltaTime).normalized;
                }

                if (m_SyncVelocity)
                {
                    m_Rigidbody.velocity +=
                        m_InterpolationState.VelocityDelta * deltaTime;
                }

                if (m_SyncAngularVelocity)
                {
                    m_Rigidbody.angularVelocity +=
                        m_InterpolationState.AngularVelocityDelta * deltaTime;
                }
            }
        }
    }

    bool NetworkVariablesInitialized()
    {
        return netVelocity.Settings.WritePermission == NetworkVariablePermission.OwnerOnly;
    }

    /// <summary>
    /// Get the surface velocity of this object at a given position using the smoothed velocity values from the
    /// networkrigidbody. 
    /// </summary>
    /// <param name="worldPos">Position on the surface of the object.</param>
    /// <returns>Velocity at the given position on this rigidbody.</returns>
    public Vector3 GetVelocityAtPoint(Vector3 worldPos)
    {
        Vector3 startingVel = m_Rigidbody.velocity;
        Vector3 startingAngVel = m_Rigidbody.angularVelocity;

        m_Rigidbody.velocity = this.netVelocity.Value;
        m_Rigidbody.angularVelocity = this.netAngularVelocity.Value;
        Vector3 vel = m_Rigidbody.GetPointVelocity(worldPos);

        m_Rigidbody.velocity = startingVel;
        m_Rigidbody.angularVelocity = startingAngVel;

        return vel;
    }

    bool TryUpdate(NetworkVariableVector3 variable, Vector3 value)
    {
        var current = variable.Value;
        if (Mathf.Approximately(current.x, value.x)
            && Mathf.Approximately(current.y, value.y)
            && Mathf.Approximately(current.z, value.z))
        {
            return false;
        }

        if (IsOwner)
        {
            variable.Value = value;
        }
        return true;
    }

    bool TryUpdate(NetworkVariableQuaternion variable, Quaternion value)
    {
        var current = variable.Value;
        if (Mathf.Approximately(current.x, value.x)
            && Mathf.Approximately(current.y, value.y)
            && Mathf.Approximately(current.z, value.z)
            && Mathf.Approximately(current.w, value.w))
        {
            return false;
        }

        if (IsOwner)
        {
            variable.Value = value;
        }
        return true;
    }
}