using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class Data
    {
        public int Cost;
        public DateTime Date;
        public string Name;
        public string Time;
        public byte[] Photo;

        public Data(int cost, DateTime date, string name, string time, byte[] photo)
        {
            Cost = cost;
            Date = date;
            Name = name;
            Time = time;
            Photo = photo;
        }
    }

    public enum ExpenseType
    {
        Expense,
        Income,
        None
    }
}