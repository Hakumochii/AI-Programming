using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/CollisionEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "CollisionEventChannel", message: "check triggercollision", category: "Events", id: "8f46bde27b4c2d10f0b8874a4b1f27c3")]
public sealed partial class CollisionEventChannel : EventChannel { }

