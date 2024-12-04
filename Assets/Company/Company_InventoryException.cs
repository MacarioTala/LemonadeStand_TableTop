using System;

[Serializable]
    public class Company_InventoryException : Exception
    {
        public Company_InventoryException(string message) : base(message)
        {
        }
    }
