using System;

namespace DsEvangelionTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if ((args == null) || (args.GetLength(0) < 5))
            {
                printInfo();
                return;
            }

            string format = args[1], outputFile = args[args.GetLength(0) - 1];
            if ((format != "bin") && (format != "png"))
            {
                Console.WriteLine("Unsupported file format: {0}. Supported values are bin, png.", format);
                return;
            }

            switch (args[0])
            {
                case "n":
                    int width, height;

                    if (!(int.TryParse(args[2], out width) && int.TryParse(args[3], out height)))
                    {
                        Console.WriteLine("Error parsing width and height.");
                        return;
                    }

                    try
                    {
                        var image = new EvangelionImage(width, height);
                        image.SaveToFile(outputFile, (format == "bin" ? ImageType.Binary : ImageType.Png));
                    }
                    catch (Exception EX)
                    {
                        Console.WriteLine("Error: {0}", EX.Message);
                    }

                    return;
                case "c":
                    width = 0;
                    if ((format == "bin") && (!int.TryParse(args[2], out width)))
                    {
                        Console.WriteLine("Width required for binary input.");
                        return;
                    }
                    string outputFormat = args[args.GetLength(0) - 2],
                        inputFile = args[args.GetLength(0) - 3];

                    if ((outputFormat != "bin") && (outputFormat != "png"))
                    {
                        Console.WriteLine("Unsupported file format: {0}. Supported values are bin, png.", outputFormat);
                        return;
                    }

                    try
                    {
                        var image = new EvangelionImage(inputFile, (format == "bin" ? ImageType.Binary : ImageType.Png), width);
                        image.SaveToFile(outputFile, (outputFormat == "bin" ? ImageType.Binary : ImageType.Png));
                    }
                    catch (Exception EX)
                    {
                        Console.WriteLine("Error: {0}", EX.Message);
                    }
                    return;

                default:
                    Console.WriteLine("Unrecognized parameter: {0}. Supported values are: n, c.", args[0]);
                    return;
            }
        }

        private static void printInfo()
        {
            Console.WriteLine("DS Evangelion image conversion tool.");
            Console.WriteLine("Made by z3t.");
            Console.WriteLine("  ");
            Console.WriteLine("Usage:");
            Console.WriteLine("  ds-evangelion-tool n <bin | png> <width> <height> <filename>");
            Console.WriteLine("  Creates a new blank image and saves it");
            Console.WriteLine("  ");
            Console.WriteLine("  ");
            Console.WriteLine("  ds-evangelion-tool c <bin | png> [<width>] <input file> <bin | png> <output file>");
            Console.WriteLine("  Converts an <input file> image to <output file>");
            Console.WriteLine("  <width> required for binary format");
        }
    }
}