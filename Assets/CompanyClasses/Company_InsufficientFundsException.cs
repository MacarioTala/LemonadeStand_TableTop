using System;

[Serializable]
    public class Company_InsufficientFundsException : Exception
    {
        public Company_InsufficientFundsException(string message) : base(message)
        {
        }
    }