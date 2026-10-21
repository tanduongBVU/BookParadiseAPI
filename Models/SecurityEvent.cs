using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookParadise.Models
{
    [Table("security_events", Schema = "quan_ly_ban_sach")]
    public class SecurityEvent
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("event_type")]
        public string EventType { get; set; } = string.Empty;

        [Required]
        [Column("source_ip")]
        public string SourceIp { get; set; } = "127.0.0.1";

        [Column("description")]
        public string? Description { get; set; }

        [Column("risk_score")]
        public int RiskScore { get; set; } = 0;

        [Column("severity")]
        public string Severity { get; set; } = "Info";

        [Column("detection_status")]
        public string DetectionStatus { get; set; } = "Pending";

        [Column("review_status")]
        public string ReviewStatus { get; set; } = "New";

        [Column("is_simulated")]
        public bool IsSimulated { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("analysis_result")]
        public string? AnalysisResult { get; set; }
    }
}