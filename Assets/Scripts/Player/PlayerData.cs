using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData: IEquatable<PlayerData>, INetworkSerializable
{
    // General player information
    public ulong clientId;
    public int colorId;
    public PlayerState playerState;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;

    // Equipment information
    public EquipmentType primaryEquipment;
    public EquipmentType secondaryEquipment;

    public bool Equals(PlayerData other) => clientId == other.clientId &&
        playerState == other.playerState &&
        colorId==other.colorId && 
        playerName == other.playerName &&
        playerId == other.playerId &&
        primaryEquipment == other.primaryEquipment &&
        secondaryEquipment == other.secondaryEquipment;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref primaryEquipment);
        serializer.SerializeValue(ref secondaryEquipment);
        serializer.SerializeValue(ref playerState);
    }
}

public enum PlayerState
{
    Runner = 0,
    Chaser = 1,
    Dead = 2
}
