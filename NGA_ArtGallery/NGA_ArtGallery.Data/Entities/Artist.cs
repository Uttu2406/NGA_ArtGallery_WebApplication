using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace NGA_ArtGallery.Data.Entities
{
    // We are using the "Gallery" schema to keep it separate from Identity tables
    [Table("Artists", Schema = "Gallery")]
    public class Artist
    {
        [Key]
        public int ArtistID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Biography { get; set; }

        public DateTime BirthDate { get; set; }

        public string Nationality { get; set; }

        public string Website { get; set; }

        public string ContactInformation { get; set; }



        // One to many reltion between Artist and Artwork
        public virtual ICollection<Artwork> Artworks { get; set; }
    }
}
