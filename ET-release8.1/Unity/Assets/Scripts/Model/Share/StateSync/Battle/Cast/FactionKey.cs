using System;

namespace ET
{
    public enum FactionKeyType : byte
    {
        Static = 1,
        Team = 2,
        Player = 3,
    }

    public readonly struct FactionKey : IEquatable<FactionKey>
    {
        public FactionKeyType Type { get; }
        public long Id { get; }

        public FactionKey(FactionKeyType type, long id)
        {
            this.Type = type;
            this.Id = id;
        }

        public bool Equals(FactionKey other) => Type == other.Type && Id == other.Id;
        public override bool Equals(object obj) => obj is FactionKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Type, Id);
        public static bool operator ==(FactionKey left, FactionKey right) => left.Equals(right);
        public static bool operator !=(FactionKey left, FactionKey right) => !left.Equals(right);
        
    }

    public static class CampConst
    {
        [StaticField]
        public static readonly FactionKey PlayerCamp = new(FactionKeyType.Static, (long)CampType.CampA);
        [StaticField]
        public static readonly FactionKey MonsterCamp = new(FactionKeyType.Static, (long)CampType.CampB);
    }
}
