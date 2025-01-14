using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Eski Þifre")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluðunda olmalýdýr.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Þifre")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Þifreyi Onayla")]
        [Compare("NewPassword", ErrorMessage = "Yeni þifre ve onay þifresi eþleþmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}
