using IGDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using poplensMediaApi.Contracts;
using poplensMediaApi.Data;
using poplensMediaApi.Helpers;
using poplensMediaApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new VectorJsonConverter());
}); ;
builder.Services.AddDbContext<MediaDbContext>(options =>
    options.UseNpgsql("Host=postgresMedia;Port=5432;Username=postgre;Password=postgre;Database=Media"));

builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<IFilmService, FilmService>(); 
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddSingleton<IGDBClient>(provider => new IGDBClient(
    clientId: "7ggnncnbidv2kboz18m5p0tgpr1o5c",
    clientSecret: "ldajb2z5anralfd2n3isowkg3ohdf2"
));
builder.Services.AddHttpClient<IBookService, BookService>();
builder.Services.AddScoped<IEmbeddingProxyService, EmbeddingProxyService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "YourIssuer",
            ValidAudience = "YourAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("moresimplekeyrightherefolkssssssssssssss"))
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
    dbContext.Database.Migrate(); // This applies any pending migrations
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();