using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myapp.Models
{
    public class DocumentRouting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentTypeId { get; set; }

        [ForeignKey("DocumentTypeId")]
        public DocumentType DocumentType { get; set; } = null!;

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; } = null!;

        [Required]
        public int SectionId { get; set; }

        [ForeignKey("SectionId")]
        public Section Section { get; set; } = null!;

        [Required]
        public int PlantId { get; set; }

        [ForeignKey("PlantId")]
        public Plant Plant { get; set; } = null!;

        [Required]
        public int Step { get; set; }
    }
}
