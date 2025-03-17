using IGDB;
using Microsoft.EntityFrameworkCore;
using poplensMediaApi.Contracts;
using poplensMediaApi.Data;
using poplensMediaApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<MediaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IFilmService, FilmService>(); 
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddSingleton<IGDBClient>(provider => new IGDBClient(
    clientId: "7ggnncnbidv2kboz18m5p0tgpr1o5c",
    clientSecret: "ldajb2z5anralfd2n3isowkg3ohdf2"
));
builder.Services.AddHttpClient<IBookService, BookService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();