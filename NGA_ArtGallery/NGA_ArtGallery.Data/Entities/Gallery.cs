using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGA_ArtGallery.Data.Entities
{
    [Table("Galleries", Schema = "Gallery")]
    public class Gallery
    {
        [Key]
        public int GalleryID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Location { get; set; }
        public string Curator { get; set; }
        public string OpeningHours { get; set; }
    }
}