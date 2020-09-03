using System.ComponentModel.DataAnnotations;

namespace QuickDiagrams.Api.Models.ManageViewModels
{
    public class IndexViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public string StatusMessage { get; set; }
        public string Username { get; set; }
    }
}