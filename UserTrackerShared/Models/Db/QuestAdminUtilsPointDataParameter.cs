namespace UserTrackerShared.Models.Db
{
    public class QuestAdminUtilsPointDataParameter : BaseAdminUtilsPointDataParameter
    {
        public string Database { get; set; }

        public QuestAdminUtilsPointDataParameter(string database, string? username, string field, double? value) : base(username, field, value)
        {
            Field = field.Replace(".", "_");
            Database = database;
        }
    }
}
