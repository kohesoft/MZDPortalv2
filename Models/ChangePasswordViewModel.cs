using System.ComponentModel.DataAnnotations;

namespace MZDNETWORK.Models
{
    public class ChangePasswordViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Eski Şifre (ilk kez belirliyorsanız boş bırakın)")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifreyi Onayla")]
        [Compare("NewPassword", ErrorMessage = "Yeni Şifre ve onay Şifresi eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}
