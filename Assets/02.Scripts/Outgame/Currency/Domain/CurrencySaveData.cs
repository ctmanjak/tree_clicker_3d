using System;
using Firebase.Firestore;
using UnityEngine;

[Serializable, FirestoreData]
public class CurrencySaveData : IIdentifiable
{
    [SerializeField] private string _id;
    [SerializeField] private string _type;
    [SerializeField] private double _amount;

    [FirestoreProperty("id")]
    public string Id { get => _id; set => _id = value; }

    [FirestoreProperty("type")]
    public string Type { get => _type; set => _type = value; }

    [FirestoreProperty("amount")]
    public double Amount { get => _amount; set => _amount = value; }
}
