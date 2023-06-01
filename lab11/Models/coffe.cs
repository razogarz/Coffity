namespace lab10.Models; 

using System.ComponentModel.DataAnnotations;

public class coffe
{
    [Key]
    public int Id { get; set; }
    
    [Display(Name = "ingredients")]
    public String ingredients { get; set; }
    
    [Display(Name = "method")]
    public String method { get; set; }

}

