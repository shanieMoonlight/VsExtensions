using AppSettingsAccessor.Tester;

var builder = WebApplication.CreateBuilder(args);

//=================================//

//Access appsettings data:
StartupData stData = new(builder.Configuration);
var bccAddresses = stData.EmailSection.GetBccAddresses();
var ccAddresses = stData.EmailSection.GetCcAddresses();
var defaultToAddress = stData.EmailSection.GetToAddress();

var maxSize = stData.GetMaxSize();

var defaultLogLevel = stData.LoggingSection.LogLevelSection.GetDefault();
var msLogLevel = stData.LoggingSection.LogLevelSection.GetMicrosoft_AspNetCore();


//---------------------------------//

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//---------------------------------//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

//=================================//