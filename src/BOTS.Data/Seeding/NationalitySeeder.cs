namespace BOTS.Data.Seeding
{
    using Microsoft.EntityFrameworkCore;

    using Models;

    internal class NationalitySeeder : ISeeder
    {
        private static readonly string[] nationalityNames =
            {
                "Afghanistan",
                "Albania",
                "Algeria",
                "Argentina",
                "Australia",
                "Austria",
                "Bangladesh",
                "Belgium",
                "Bolivia",
                "Botswana",
                "Brazil",
                "Bulgaria",
                "Cambodia",
                "Cameroon",
                "Canada",
                "Chile",
                "China",
                "Colombia",
                "Costa Rica",
                "Croatia",
                "Cuba",
                "Czech Republic",
                "Denmark",
                "Dominican Republic",
                "Ecuador",
                "Egypt",
                "El Salvador",
                "England",
                "Estonia",
                "Ethiopia",
                "Fiji",
                "Finland",
                "France",
                "Germany",
                "Ghana",
                "Greece",
                "Guatemala",
                "Haiti",
                "Honduras",
                "Hungary",
                "Iceland",
                "India",
                "Indonesia",
                "Iran",
                "Iraq",
                "Ireland",
                "Israel",
                "Italy",
                "Jamaica",
                "Japan",
                "Jordan",
                "Kenya",
                "Kuwait",
                "Laos",
                "Latvia",
                "Lebanon",
                "Libya",
                "Lithuania",
                "Madagascar",
                "Malaysia",
                "Mali",
                "Malta",
                "Mexico",
                "Mongolia",
                "Morocco",
                "Mozambique",
                "Namibia",
                "Nepal",
                "Netherlands",
                "New Zealand",
                "Nicaragua",
                "Nigeria",
                "Norway",
                "Pakistan",
                "Panama",
                "Paraguay",
                "Peru",
                "Philippines",
                "Poland",
                "Portugal",
                "Romania",
                "Russia",
                "Saudi Arabia",
                "Scotland",
                "Senegal",
                "Serbia",
                "Singapore",
                "Slovakia",
                "South Africa",
                "South Korea",
                "Spain",
                "Sri Lanka",
                "Sudan",
                "Sweden",
                "Switzerland",
                "Syria",
                "Taiwan",
                "Tajikistan",
                "Thailand",
                "Tonga",
                "Tunisia",
                "Turkey",
                "Ukraine",
                "United Arab Emirates",
                "United Kingdom",
                "United States",
                "Uruguay",
                "Venezuela",
                "Vietnam",
                "Wales",
                "Zambia",
                "Zimbabwe"
            };

        public async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.Nationalities.AnyAsync())
            {
                return;
            }

            Nationality[] nationalities = nationalityNames
                                  .Select((x, i) => new Nationality
                                  {
                                      Name = x,
                                  })
                                  .ToArray();

            await dbContext.AddRangeAsync(nationalities);
        }
    }
}
