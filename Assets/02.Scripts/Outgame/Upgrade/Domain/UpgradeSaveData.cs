using System;
using Core;
using Firebase.Firestore;
using UnityEngine;

namespace Outgame
{
    [Serializable, FirestoreData]
    public class UpgradeSaveData : IIdentifiable, ITimestamped
    {
        [SerializeField] private string _id;
        [SerializeField] private int _level;
        [SerializeField] private long _lastModified;

        [FirestoreProperty("id")]
        public string Id { get => _id; set => _id = value; }

        [FirestoreProperty("level")]
        public int Level { get => _level; set => _level = value; }

        [FirestoreProperty("lastModified")]
        public long LastModified { get => _lastModified; set => _lastModified = value; }
    }
}
