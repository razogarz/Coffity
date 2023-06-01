using System.ComponentModel.DataAnnotations;

namespace lab10.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    [Display(Name = "Login")]
    public String Login { get; set; }
    [Display(Name = "Password")]
    public String Password { get; set; }
}