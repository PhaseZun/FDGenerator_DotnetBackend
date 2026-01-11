using System;

namespace AuthApi.Models
{
    public class FDModel
{
    public string? username { get; set;}
    public int fdId { get; set; }
    public decimal Amount { get; set; }
    public double InterestRate { get; set; }
    public DateTime InvestedDate { get; set; }     // New
    public DateTime MaturityDate { get; set; }
    public int TenureMonths { get; set; }          // New
    public string? BankName { get; set; }           // New
    public string? AccountNumber { get; set; }      // New
    public string? Status { get; set; }             // e.g., Active, Matured
}

}
