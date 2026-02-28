using System.Collections.Generic;
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
        public string OpeningHours { get; set; }

        // 1. Link to the Artist who acts as the Curator
        public int ArtistID { get; set; }

        [ForeignKey("ArtistID")]
        public virtual Artist Curator { get; set; }

        // 2. Many-to-Many link to Artworks
        public virtual ICollection<Artwork> Artworks { get; set; }
    }
}