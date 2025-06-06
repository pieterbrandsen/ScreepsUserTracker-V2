using UserTrackerShared;

namespace UserTracker.Tests;
public class Company
{
    public List<Department> Departments { get; set; }
}

public class Department
{
    public List<Employee> Employees { get; set; }
}

public class Employee
{
    public string Name { get; set; }
    public Address Location { get; set; }
    public int Age { get; set; }
    public long Id { get; set; }
    public int[] Ratings { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Zip { get; set; }
    public Coordinates Geo { get; set; }
}

public class Coordinates
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class PatchDiagnostics : IDynamicPatchDiagnostics
{
    public void Error(string path, string segment, Exception ex) => Console.WriteLine($"ERROR at {path}: {segment} => {ex.Message}");
    public void Trace(string message) => Console.WriteLine("TRACE: " + message);
}

public class DynamicPatcherTests
{
    [Fact]
    public void Test_Set_Nested_Null()
    {
        var c = new Company();
        DynamicPatcher.ApplyPatch(c, "Departments[0].Employees[0].Location.City", null);
        Assert.Null(c.Departments[0].Employees[0].Location.City);
    }

    [Fact]
    public void Test_Set_Array_Index()
    {
        var c = new Company();
        DynamicPatcher.ApplyPatch(c, "Departments[0].Employees[0].Ratings[3]", 5);
        Assert.Equal(5, c.Departments[0].Employees[0].Ratings[3]);
    }

    [Fact]
    public void Test_Deep_Object_AutoCreate()
    {
        var c = new Company();
        DynamicPatcher.ApplyPatch(c, "Departments[1].Employees[1].Location.Geo.Lat", 52.1);
        Assert.Equal(52.1, c.Departments[1].Employees[1].Location.Geo.Lat);
    }

    [Theory]
    [InlineData("Departments[2].Employees[3].Age", 45)]
    [InlineData("Departments[2].Employees[3].Id", 100L)]
    [InlineData("Departments[1].Employees[0].Name", "TestName")]
    [InlineData("Departments[0].Employees[1].Location.Zip", "12345")]
    [InlineData("Departments[3].Employees[1].Location.Geo.Lng", 13.37)]
    [InlineData("Departments[0].Employees[0].Ratings[2]", 99)]
    public void Test_Multiple_Property_Patching(string path, object value)
    {
        var c = new Company();
        DynamicPatcher.ApplyPatch(c, path, value);
        // Implicitly successful if no exceptions thrown and value is set — more targeted asserts could be used.
    }

    [Fact]
    public void Test_Replace_Array()
    {
        var e = new Employee { Ratings = new int[2] { 1, 2 } };
        var c = new Company
        {
            Departments = new List<Department> {
            new Department { Employees = new List<Employee> { e } }
        }
        };
        DynamicPatcher.ApplyPatch(c, "Departments[0].Employees[0].Ratings[4]", 88);
        Assert.Equal(88, c.Departments[0].Employees[0].Ratings[4]);
    }

    [Fact]
    public void Test_Set_Object_To_Null()
    {
        var c = new Company();
        DynamicPatcher.ApplyPatch(c, "Departments[0].Employees[0].Location", null);
        Assert.Null(c.Departments[0].Employees[0].Location);
    }

    [Fact]
    public void Test_Set_Invalid_Index_Throws()
    {
        var c = new Company();
        Assert.Throws<FormatException>(() => DynamicPatcher.ApplyPatch(c, "Departments[abc].Employees[0].Age", 1));
    }

    [Fact]
    public void Test_Missing_Property_Throws()
    {
        var c = new Company();
        Assert.Throws<InvalidOperationException>(() => DynamicPatcher.ApplyPatch(c, "UnknownProp.X", 1));
    }

    [Theory]
    [InlineData("Departments[0].Employees[0].Location.Geo.Lat", 0.123)]
    [InlineData("Departments[0].Employees[0].Location.Geo.Lng", 0.456)]
    [InlineData("Departments[0].Employees[0].Location.Zip", "54321")]
    [InlineData("Departments[0].Employees[0].Name", "Alex")]
    [InlineData("Departments[0].Employees[0].Age", 30)]
    [InlineData("Departments[0].Employees[0].Id", 123456789L)]
    [InlineData("Departments[0].Employees[0].Ratings[1]", 42)]
    public void Test_Deeply_Nested_All_Types(string path, object value)
    {
        var c = new Company();
        DynamicPatcher.ApplyPatch(c, path, value);
    }
}
