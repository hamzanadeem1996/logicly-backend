using Apex.DataAccess.Models;
using CsvHelper;
using ExcelMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Apex_Api.Service
{
    public class ImportCSVService
    {
        public IEnumerable<PatientCsvResponse> ImportCSV(IFormFile file)
        {
            IEnumerable<PatientCsvResponse> records = null;
            if (file == null) return null;

            var filename = DateTime.Now.ToFileTime() + file.FileName.Replace(" ", "_");
            var path = Path.Combine($"{Common.UploadPath}/{"doc"}");
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            using (var fileStream = new FileStream(Path.Combine(path, filename), FileMode.Create))
            {
                file.CopyTo(fileStream);
                var isCsv = Path.GetExtension(filename) == ".csv";
                if (isCsv)
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            csv.Configuration.HeaderValidated = null;
                            csv.Configuration.PrepareHeaderForMatch =
                                (header, index) => header.ToLower().Replace(" ", "");
                            csv.Configuration.MissingFieldFound = null;
                            var records1 = csv.GetRecords<dynamic>();
                            records = csv.GetRecords<PatientCsvResponse>().ToList();
                        }
                    }
                }
                else
                {
                    var importer = new ExcelImporter(file.OpenReadStream());
                    var sheet = importer.ReadSheet();
                    importer.Configuration.SkipBlankLines = true;
                    records = sheet.ReadRows<PatientCsvResponse>().ToArray();
                }
            }
            return records;
        }
    }
}