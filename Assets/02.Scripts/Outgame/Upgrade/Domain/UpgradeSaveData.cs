using System;
using Firebase.Firestore;
using UnityEngine;

namespace Outgame
{
    [Serializable, FirestoreData]
    public class UpgradeSaveData : IIdentifiable
    {
        [SerializeField] private string _id;
        [SerializeField] private int _level;

        [FirestoreProperty("id")]
        public string Id { get => _id; set => _id = value; }

        [FirestoreProperty("level")]
        public int Level { get => _level; set => _level = value; }
    }
}
