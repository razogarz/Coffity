namespace lab10.Models; 

using System.ComponentModel.DataAnnotations;

public class recipe {
    
    [Key]
    public int Id { get; set; }
    
    [Display(Name = "name")]
    public String name { get; set; }
    
    [Display(Name = "img")]
    public String img { get; set; }
    
    [Display(Name = "description")]
    public String description { get; set; }

}