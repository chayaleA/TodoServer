using TodoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenPolicy", policy => { policy.WithOrigins("*").AllowAnyHeader()
    .AllowAnyMethod(); });
});


builder.Services.AddDbContext<ToDoDbContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
 new MySqlServerVersion(new Version(8, 0, 36))));


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Keep track of your tasks", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
});

app.UseCors("OpenPolicy");

app.MapGet("/", () => "Hello World!");

app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync());

app.MapPost("/items", async (ToDoDbContext db, Item todo) =>
{
    await db.Items.AddAsync(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todo/{todo.Id}", todo);
});

app.MapGet("/items/{id}", async (ToDoDbContext db, int id) => await db.Items.FindAsync(id));

app.MapPut("/items/{id}", async (ToDoDbContext db, Item updateTodo, int id) =>
{
    var todo = await db.Items.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = updateTodo.Name;
    todo.IsComplete = updateTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var todo = await db.Items.FindAsync(id);
    if (todo is null)
    {
        return Results.NotFound();
    }
    db.Items.Remove(todo);
    await db.SaveChangesAsync();

    return Results.Ok();

});

app.Run();
