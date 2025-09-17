namespace UserTrackerShared.Models.Db
{
    public abstract class BaseAdminUtilsPointDataParameter
    {
        public string? Username { get; set; }
        public string Field { get; set; }
        public double? Value { get; set; }

        public BaseAdminUtilsPointDataParameter(string? username, string field, double? value)
        {
            Username = username;
            Field = field;
            Value = value;
        }
    }
}
