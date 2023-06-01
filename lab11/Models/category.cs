namespace lab10.Models; 

using System.ComponentModel.DataAnnotations;
    
public class category {

    [Display(Name = "id")]
    public int caffe_id { get; set; }
    
    [Display(Name = "category")]
    public String category_name { get; set; }
}