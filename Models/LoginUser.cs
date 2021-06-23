using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAccounts.Models
{
    
    [NotMapped] // don't add table to db
    public class LoginUser
    {
        [Display(Name = "Email Address:")]
        [Required(ErrorMessage = "is required.")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string EmailLogin { get; set; }

        [Required(ErrorMessage = "is required.")]
        [MinLength(8, ErrorMessage = "must be at least 8 characters")]
        [DataType(DataType.Password)] // auto fills input type attr
        public string PasswordLogin { get; set; }
    }
}