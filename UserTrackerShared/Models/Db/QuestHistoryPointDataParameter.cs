namespace UserTrackerShared.Models.Db
{
    public class QuestHistoryPointDataParameter : BaseHistoryPointDataParameter
    {
        public string Database { get; set; }

        public QuestHistoryPointDataParameter(string database, string shard, string room, long tick, long timestamp, string username, string field, double? value) : base(shard, room, tick, timestamp, username, database, field, value)
        {
            Field = field.Replace(".", "_");
            Database = database;
        }
    }
}
