namespace UserTrackerShared.Models.Db
{
    public abstract class BaseHistoryPointDataParameter
    {
        public string Shard { get; set; }
        public string Room { get; set; }
        public long Tick { get; set; }
        public long Timestamp { get; set; }
        public string Username { get; set; }
        public string Measurement { get; set; }
        public string Field { get; set; }
        public double? Value { get; set; }

        public BaseHistoryPointDataParameter(string shard, string room, long tick, long timestamp, string username, string measurement, string field, double? value)
        {
            Shard = shard;
            Room = room;
            Tick = tick;
            Timestamp = timestamp;
            Username = username;
            Measurement = measurement;
            Field = field;
            Value = value;
        }
    }
}
