using System;
using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Models
{
    public class Transaction
    {
        [Key] // the below prop is the primary key, [Key] is not needed if named with pattern: ModelNameId
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "is required.")]
        [Display(Name = "Deposit/Withdraw")]
        public decimal Amount  { get; set;}

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /* Foreign Keys and Navigation Properties for Relationships */
        public int UserId { get; set; }
        public User User { get; set; }
    }
}