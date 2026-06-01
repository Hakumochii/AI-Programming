using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/EventChannelBase")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "EventChannelBase", message: "check for collision", category: "Events", id: "2666e49c23cd158ddcb84726652d86ca")]
public sealed partial class EventChannelBase : EventChannel { }

