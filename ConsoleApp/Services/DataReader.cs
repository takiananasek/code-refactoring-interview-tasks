namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ConsoleApp.Models;
    public class DataReader
    {
        readonly List<ImportedObject> ImportedObjects = new List<ImportedObject>();
        public void ImportAndPrintData(string fileToImport)
        {
            var streamReader = new StreamReader(fileToImport);

            var importedLines = new List<string>();

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                importedLines.Add(line);
            }

            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';');
                var valuesToAssign = new List<string>();

                for(int j = 0 ; j < 7 ; j++)
                {
                    if (j < values.Length)
                    {
                        valuesToAssign.Add(values[j]);
                        continue;
                    }
                    valuesToAssign.Add("");   
                }
                var importedObject = new ImportedObject()
                {
                    Type = valuesToAssign[0],
                    Name = valuesToAssign[1],
                    Schema = valuesToAssign[2],
                    ParentName = valuesToAssign[3],
                    ParentType = valuesToAssign[4],
                    DataType = valuesToAssign[5],
                    IsNullable = valuesToAssign[6]
                };
               
                ImportedObjects.Add(importedObject);
            }

            // clear and correct imported data

            ClearAndCorrect(ImportedObjects);

            // assign number of children

            AssignNumberOfChildren(ImportedObjects);

            Print(ImportedObjects);

            Console.ReadLine();
        }

        public void ClearAndCorrect(List<ImportedObject> ImportedObjects)
        {
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.Type = importedObject.Type?.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                importedObject.Name = importedObject.Name?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.Schema = importedObject.Schema?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentName = importedObject.ParentName?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType?.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
            }
        }

        public void AssignNumberOfChildren(List<ImportedObject> ImportedObjects)
        {
            for (int i = 0; i < ImportedObjects.Count(); i++)
            {
                var importedObject = ImportedObjects.ToArray()[i];
                foreach (var impObj in ImportedObjects)
                {
                    if (impObj.ParentType == importedObject.Type && impObj.ParentName == importedObject.Name)
                    {
                        importedObject.NumberOfChildren += 1;
                    }
                }
            }
        }

        public void Print(List<ImportedObject> ImportedObjects)
        {
            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    // print all database's tables
                    foreach (var table in ImportedObjects)
                    {
                        if (table.ParentType?.ToUpper() == database.Type)
                        {
                            if (table.ParentName == database.Name)
                            {
                                Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                                // print all table's columns
                                foreach (var column in ImportedObjects)
                                {
                                    if (column.ParentType?.ToUpper() == table.Type)
                                    {
                                        if (column.ParentName == table.Name)
                                        {
                                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
