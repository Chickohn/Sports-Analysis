using System.Globalization;
using CsvHelper;
using Sports_Analysis.Models;
using Sports_Analysis.Data;

public class CsvImportService
{
    private readonly FootballDbContext _context;

    public CsvImportService(FootballDbContext context)
    {
        _context = context;
    }

    public void ImportFootballMatches(string csvFilePath)
    {
        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<FootballMatchMap>();
        var records = csv.GetRecords<FootballMatch>().ToList();

        _context.FootballMatches.AddRange(records);
        _context.SaveChanges();
    }
}