using System;

namespace TmTest
{
    public class Stats
    {
        public int ParticipantId { get; set; }
        public int Map { get; set; }
        public DateTime StartTime { get; set; }
        public int Retries { get; set; }
        public bool TimerTriggered { get; set; }
        public TimeSpan TimerInterval { get; set; }
        public DateTime EndTime { get; set; }
    }
}