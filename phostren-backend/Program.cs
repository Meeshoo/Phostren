using Microsoft.AspNetCore.Mvc;
using ExifLib;
using System.Security.Cryptography.X509Certificates;


string BASE_URL = "http://127.0.0.1:5500";
string API_URL = "http://127.0.0.1:8000";
string PHOTO_LOCATION = "../phostren-frontend/photos";


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();

var app = builder.Build();


app.UseCors( x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithOrigins(BASE_URL));


// Endpoints
app.MapGet("/getallphotos", () => {

    DateTime now = DateTime.Now;
    List<Photo> photos = new List<Photo>();

    foreach (string file in Directory.GetFiles(PHOTO_LOCATION)) {

        ExifReader reader = new ExifReader(file);

        DateTime datePictureTaken;
        if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out datePictureTaken)) {

            if (datePictureTaken > now.AddDays(-1)) {
                
                Photo photo = new() {
                    filename = file,
                    dateTaken = datePictureTaken
                };
                
                photos.Add(photo);
            }
        }
    }

    photos = photos.OrderByDescending(x => x.dateTaken).ToList();

    return galleryTemplate(photos);

});

app.MapGet("/getlatestphoto", () => {

    Photo latestPhoto = getLatestPhoto();

    return @$"<img class=""photo big_photo"" src=""{latestPhoto.filename}""></img>";

});

app.MapGet("/getwidgetphoto", ([FromQuery(Name = "display_duration")] string displayDuration) => {

    Photo latestPhoto = getLatestPhoto();

    DateTime now = DateTime.Now;

    // HOW TO DO THIS
    if (latestPhoto.dateTaken > now.AddSeconds(-15)){
        return @$"<img class=""photo big_photo widget_photo"" src=""{latestPhoto.filename}""></img>";
    } else {
        return "";
    }

});

app.MapGet("/imagecleanup", () => {

        DateTime now = DateTime.Now;

        foreach (string file in Directory.GetFiles(PHOTO_LOCATION)) {

            ExifReader reader = new ExifReader(file);

            DateTime datePictureTaken;
            if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out datePictureTaken)) {

                if (datePictureTaken < now.AddDays(-1)) {
                    File.Delete(file);
                }
            }
        }
});


// Functions
Photo getLatestPhoto() {

    string latestPhotoFilename = "";
    DateTime latestPhotoDateTime = DateTime.Now.AddYears(-30);

    foreach (string file in Directory.GetFiles(PHOTO_LOCATION))
    {

        ExifReader reader = new ExifReader(file);

        DateTime datePictureTaken;

        if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out datePictureTaken))
        {

            if (datePictureTaken > latestPhotoDateTime)
            {
                latestPhotoDateTime = datePictureTaken;
                latestPhotoFilename = file;
            }
        }
    }

    Photo photo = new() {
        filename = latestPhotoFilename,
        dateTaken = latestPhotoDateTime
    };

    return photo;
};


// Templates
static string galleryTemplate(List<Photo> photos) {

    string response = @$"
        
    ";

    foreach (Photo photo in photos) {
        response += @$"<img class=""gallery_photo"" src=""{photo.filename}""></img>";
    }

    return response;

};


app.Run();


public struct Photo {
    public string filename;
    public DateTime dateTaken;
}