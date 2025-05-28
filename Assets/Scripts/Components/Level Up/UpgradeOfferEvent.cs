using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct UpgradeOfferEvent : IComponentData
{
    public NativeList<UpgradeOptionStruct> Options;
}
