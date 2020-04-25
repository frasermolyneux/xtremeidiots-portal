using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using XI.CommonTypes;

namespace XI.Portal.Players.Dto
{
    public class AdminActionDto
    {
        public AdminActionDto()
        {
            Created = DateTime.UtcNow;
        }

        public Guid AdminActionId { get; set; }
        public Guid PlayerId { get; set; }
        public GameType GameType { get; set; }
        public string Username { get; set; }
        public string Guid { get; set; }
        public AdminActionType Type { get; set; }

        [Required]
        [DisplayName("Reason")]
        [MinLength(3, ErrorMessage = "You must enter a reason for the admin action")]
        public string Text { get; set; }

        public DateTime? Expires { get; set; }
        public int ForumTopicId { get; set; }
        public DateTime Created { get; set; }

        public string AdminId { get; set; }
        public string AdminName { get; set; }
    }
}