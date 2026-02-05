using System;
using Core;
using Firebase.Firestore;
using UnityEngine;

namespace Outgame
{
    [Serializable, FirestoreData]
    public class CurrencySaveData : IIdentifiable, ITimestamped
    {
        [SerializeField] private string _id;
        [SerializeField] private string _type;
        [SerializeField] private double _amount;
        [SerializeField] private long _lastModified;

        [FirestoreProperty("id")]
        public string Id { get => _id; set => _id = value; }

        [FirestoreProperty("type")]
        public string Type { get => _type; set => _type = value; }

        [FirestoreProperty("amount")]
        public double Amount { get => _amount; set => _amount = value; }

        [FirestoreProperty("lastModified")]
        public long LastModified { get => _lastModified; set => _lastModified = value; }
    }
}
