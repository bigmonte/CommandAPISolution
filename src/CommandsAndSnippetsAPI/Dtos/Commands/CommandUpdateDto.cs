using System.ComponentModel.DataAnnotations;

namespace CommandsAndSnippetsAPI.Dtos
{
    public class CommandUpdateDto
    {
        // Equal as CommandCreateDto
        
        [Required][MaxLength(250)]
        public string HowTo { get; set; }
        
        [Required]
        public string Platform { get; set; }
        
        [Required]
        public string CommandLine { get; set; }

    }
}