//var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

/*builder.Services.AddCors(o => o.AddPolicy("Cors", builder =>
{
    builder
        .WithOrigins("http://127.0.0.1:7296")
        .WithHeaders("*")
        .WithMethods("*")
        .AllowCredentials();
}));*/

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.UseCors(builder =>
{
    builder
        .WithOrigins("*")
        .WithHeaders("*")
        .WithMethods("*");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }