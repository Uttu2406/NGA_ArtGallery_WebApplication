using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGA_ArtGallery.Data.Entities
{
    [Table("Artworks", Schema = "Gallery")]
    public class Artwork
    {
        [Key]
        public int ArtworkID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime CreationDate { get; set; }

        public string Medium { get; set; }

        public string ImageURL { get; set; }



        // Many to one relationship between Artwork and Artist
        public int ArtistID { get; set; }

        [ForeignKey("ArtistID")]
        public virtual Artist Artist { get; set; }
    }
}
