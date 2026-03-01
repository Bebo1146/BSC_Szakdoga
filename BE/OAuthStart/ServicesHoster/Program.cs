using TokenValidation.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add token validation (JWT Bearer authentication + authorization)
builder.Services.AddTokenValidation(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication middleware (must be before authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
