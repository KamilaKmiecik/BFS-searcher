using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Document = iTextSharp.text.Document;
using Font = System.Drawing.Font;
using Image = iTextSharp.text.Image;

namespace v
{
    internal class Program
    {
        static string filePath = "Raport.txt";
        static StreamWriter writer = new StreamWriter(filePath);

        static void Main(string[] args)
        {
            Random random = new Random();

            writer.WriteLine("|GRAPH STATISTICS RAPORT|");
            Console.Write("Input n: ");
            int n = int.Parse(Console.ReadLine());
            int[,] a = new int[n, n];
            double[,] w = new double[n, n];
            writer.WriteLine("Matrix size: " + n + "x" + n);

            Console.Write("Input p: ");
            double p = double.Parse(Console.ReadLine());
            writer.WriteLine("Vertex existence propability: " + p);

            a = FillMatrix(a, p, random, w);
            SaveToBitmap(a);
            SaveToRaport(a);
            AddBFSToRaport(a);
            ConvertToPdf(filePath);
            writer.Close();
        }
        static int[] BFS(int[,] graph, int start)
        {
            int n = graph.GetLength(0); // number of vertices
            bool[] visited = new bool[n];
            int[] distances = new int[n];
            Queue<int> queue = new Queue<int>();

            visited[start] = true;
            queue.Enqueue(start);
            distances[start] = 0;

            while (queue.Count != 0)
            {
                int s = queue.Dequeue();

                for (int i = 0; i < n; i++)
                {
                    if (graph[s, i] == 1 && !visited[i])
                    {
                        visited[i] = true;
                        distances[i] = distances[s] + 1;
                        queue.Enqueue(i);
                    }
                }
            }

            return distances;
        }

        static int[,] FillMatrix(int[,] matrix, double propability, Random random, double[,] weight)
        {
            if (propability < 0 || propability > 1)
            {
                Console.WriteLine("Propability contains within [0,1]");
            }

            Console.WriteLine();
            for (int i = 0; i < matrix.GetLength(0) - 1; i++)
            {
                for (int j = i + 1; j < matrix.GetLength(1); j++)
                {
                    double a = random.NextDouble();

                    if (a >= propability || i == j)
                    {
                        matrix[i, j] = matrix[j, i] = 0;
                    }
                    else
                    {
                        matrix[i, j] = matrix[j, i] = 1;
                    }
                }
            }
            Console.WriteLine();

            return matrix;
        }

        static void SaveToBitmap(int[,] matrix)
        {
            // Define the graph layout
            int vertexRadius = 20;
            int ellipseWidth = 500;
            int ellipseHeight = 300;
            int xOffset = 100;
            int yOffset = 100;

            // Create matrix bitmap to draw on
            int bitmapWidth = ellipseWidth + (2 * xOffset);
            int bitmapHeight = ellipseHeight + (2 * yOffset);
            Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight);

            // Create matrix graphics object from the bitmap
            Graphics g = Graphics.FromImage(bitmap);

            // Clear the background
            g.Clear(Color.White);

            // Calculate the coordinates of the vertices on the ellipse
            PointF[] vertexCoords = new PointF[matrix.GetLength(0)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                double angle = 2 * Math.PI * i / matrix.GetLength(0);
                float x = (float)(ellipseWidth / 2 * Math.Cos(angle) + xOffset + ellipseWidth / 2);
                float y = (float)(ellipseHeight / 2 * Math.Sin(angle) + yOffset + ellipseHeight / 2);
                vertexCoords[i] = new PointF(x, y);
            }

            // Draw the edges
            Pen pen = new Pen(Color.Black, 2);
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    PointF vertex1 = vertexCoords[i];
                    if (matrix[i, j] == 1)
                    {
                        if (matrix[i, j] == 1)
                        {
                            PointF vertex2 = vertexCoords[j];
                            g.DrawLine(pen, vertex1, vertex2);
                        }
                    }
                }
            }

            // Draw the vertices
            int nodeNumber = 1;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    PointF vertex = vertexCoords[i];
                    g.FillEllipse(Brushes.DarkSlateBlue, vertex.X - vertexRadius, vertex.Y - vertexRadius, vertexRadius * 2, vertexRadius * 2);
                    g.DrawString(nodeNumber.ToString(), new Font("Arial", 12), Brushes.White, vertex.X - 8, vertex.Y - 10);
                }
                nodeNumber++;
            }

            // Save the bitmap to matrix file
            string outputFilePath = "graph.bmp";
            bitmap.Save(outputFilePath);

        }

        static void SaveToRaport(int[,] matrix)
        {
            try
            {
                var neighbors = GetNeighbors(matrix);
                var degrees = GetDegrees(matrix);
                var density = GetDensity(matrix);

                using (writer)
                {
                    writer.WriteLine("\nList of degrees");
                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        writer.Write($"{degrees[i]}   ");
                    }

                    writer.WriteLine($"\nDensity = {density}");

                    writer.WriteLine("\nList of neighbors\n");
                    foreach (var bby in neighbors)
                    {
                        writer.Write(bby.Key + "|");
                        foreach (int item in bby.Value)
                        {
                            writer.Write("   " + item);
                        }
                        writer.WriteLine();
                    }
                }

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("\n\nMatrix 0-1 including degrees\n");
                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        int degree = 0;
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            writer.Write(matrix[i, j] + "   ");
                            if (matrix[i, j] == 1) degree++;
                        }

                        writer.Write($"| {degree}");
                        writer.WriteLineAsync(); writer.WriteLine();
                    }
                    Console.WriteLine("Matrix saved to raport!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void AddBFSToRaport(int[,] matrix)
        {
            Random rand = new Random();
            int start = rand.Next(0, matrix.GetLength(0));
            int[] distances = BFS(matrix, start);
            string[] distancesRomanic = new string[distances.Length];

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"\nBFS\n");
                for (int i = 0; i < distances.Length; i++)
                    writer.WriteLine($"|Distance from: {start + 1}| To vertex : {i + 1} | {distances[i]}|");


            }
            Console.WriteLine("BFS analysis saved to raport!");
        }

        static void ConvertToPdf(string txtFilePath)
        {
            string pdfFilePath = txtFilePath.Replace("txt", "pdf");

            // create a new PDF document
            using (var document = new Document())
            {
                // create a new PDF writer
                using (var writer = PdfWriter.GetInstance(document, new FileStream(pdfFilePath, FileMode.Create)))
                {
                    // open the PDF document
                    document.Open();

                    // create a new font
                    var font = FontFactory.GetFont(BaseFont.COURIER, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);

                    // read the text file and write the content to the PDF document
                    using (var reader = new StreamReader(txtFilePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            document.Add(new Paragraph(line, font));
                        }
                    }

                    Image bmpImage = Image.GetInstance("graph.bmp");
                    bmpImage.ScaleToFit(document.PageSize.Width - document.LeftMargin - document.RightMargin, document.PageSize.Height - document.TopMargin - document.BottomMargin);
                    document.NewPage();
                    document.Add(bmpImage);
                    var paragraph = new Paragraph("Fig 1. Graph visualisation", font);
                    paragraph.Alignment = Element.ALIGN_CENTER;
                    document.Add(paragraph);
                    // close the PDF document
                    document.Close();
                }
                Console.WriteLine("Raport succesuly saved!\nGo to *\\bin\\Debug to find the *.bmp, *.txt as well as *.pdf file");
                Process.Start(pdfFilePath);
            }
        }
        static Dictionary<int, List<int>> GetNeighbors(int[,] a)
        {
            Dictionary<int, List<int>> neighbors = new Dictionary<int, List<int>>();
            List<int> n = new List<int>();

            for (int i = 0; i < a.GetLength(0); i++)
            {
                n = new List<int>();
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    if (a[i, j] == 1)
                    {
                        n.Add(j + 1);
                    }
                }
                if (n.Count != 0) neighbors.Add(i + 1, n);
            }

            return neighbors;
        }

        static int[] GetDegrees(int[,] matrix)
        {
            int[] degrees = new int[matrix.GetLength(0)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                degrees[i] = 0;
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    degrees[i] += matrix[i, j];
                }
            }
            return degrees;
        }

        static double GetDensity(int[,] matrix)
        {
            double density = 0;
            int m = GetNeighbors(matrix).Count;
            int[] degrees = GetDegrees(matrix);

            density = m / (0.5 * matrix.GetLength(0) * (matrix.GetLength(0) - 1));

            return density;
        }
    }
}
