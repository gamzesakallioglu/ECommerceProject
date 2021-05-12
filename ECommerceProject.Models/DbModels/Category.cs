using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Models.DbModels
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required (ErrorMessage ="Bu alan boş geçilemez")]
        [StringLength(100, MinimumLength =3, ErrorMessage ="Lütfen en az 3 karakter giriniz")]
        public string CategoryName { get; set; }
    }
}
