using System;

namespace AuthApi.Models
{
    public class FDModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public double InterestRate { get; set; }
        public DateTime MaturityDate { get; set; }
    }
}
