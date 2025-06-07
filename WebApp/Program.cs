using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (HttpContext context) =>
{
    if (context.Request.Method == "GET") //READ
    {
        if (context.Request.Path.StartsWithSegments("/"))
        {

            await context.Response.WriteAsync($"The method is: {context.Request.Method}\r\n");
            await context.Response.WriteAsync($"The URL is: {context.Request.Path}\r\n");

            await context.Response.WriteAsync($"\r\nHeaders:\r\n");
            foreach (var key in context.Request.Headers.Keys)
            {
                await context.Response.WriteAsync($"{key}: {context.Request.Headers[key]}\r\n");
            }
        }

        else if (context.Request.Path.StartsWithSegments("/employees"))
        {
            var employees = EmployeesRepository.GetEmployees();

            foreach (var employee in employees)
            {
                await context.Response.WriteAsync($"{employee.Name}: {employee.Position}\r\n");
            }
        }
    }

    else if (context.Request.Method == "POST") //CREATE
    {
        if (context.Request.Path.StartsWithSegments("/employees"))
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var employee = JsonSerializer.Deserialize<Employee>(body);

            EmployeesRepository.AddEmployee(employee);
        }
    }

    else if (context.Request.Method == "PUT") //UPDATE
    {
        if (context.Request.Path.StartsWithSegments("/employees"))
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            var employee = JsonSerializer.Deserialize<Employee>(body);

            var result = EmployeesRepository.UpdateEmployee(employee);
            if (result)
            {
                await context.Response.WriteAsync("Employee updated successfully.");
            }
            else
            {
                await context.Response.WriteAsync("Employee not found.");
            }
        }
    }


    /*  QUERY STRING  */
    foreach (var key in context.Request.Query.Keys)
    {
        await context.Response.WriteAsync($"{key}: {context.Request.Query[key]}\r\n");
    }

    //await context.Response.WriteAsync(context.Request.QueryString.ToString());
});

app.Run();

static class EmployeesRepository
{
    private static List<Employee> employees = new List<Employee>
    {
        new Employee(1, "John Doe", "Engineer", 60000),
        new Employee(2, "Jane Smith", "Manager", 75000),
        new Employee(3, "Sam Brown", "Technician", 50000),
    };

    public static List<Employee> GetEmployees() => employees;
    public static void AddEmployee(Employee? employee)
    {
        if(employee is not null)
        {
            employees.Add(employee);
        }            
    }

    public static bool UpdateEmployee(Employee? employee)
    {
        if(employee is not null)
        {
            var emp = employees.FirstOrDefault(x => x.Id == employee.Id);
            if(emp is not null)
            {
                emp.Name = employee.Name;
                emp.Position = employee.Position;
                emp.Salary = employee.Salary;

                return true;
            }
        }
        return false;
    }
}

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public double Salary { get; set; }

    public Employee(int id, string name, string position, double salary)
    {
        Id = id;
        Name = name;
        Position = position;
        Salary = salary;
    }
}
