using System;

namespace NFe.Domain.Entities
{
    public class NfeEvent
    {
        public int Id { get; set; }
        public int NfeId { get; set; }
        public string EventType { get; set; }
        public string EventStatus { get; set; }
        public string EventData { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public NfeDocument NfeDocument { get; set; }
    }
}