using System.ComponentModel.DataAnnotations;

namespace PapyrusAlgorithms.Database
{
    public class Settings
    {
        [Key]
        public int Id { get; set; }
        public int Dimension { get; set; }
        public string Profile { get; set; }
        public string Format { get; set; }
        public int Quality { get; set; }

        public int ChunksPerDimension { get; set; }

        public int MaxZoom { get; set; }
        public int MinZoom { get; set; }
    }
}