# BFS-searcher
The program code you provided generates a random graph based on the user's input, saves it as a bitmap, and then saves the graph statistics report as a text file and converts it to a PDF file. The report includes the matrix size, vertex existence probability, the adjacency matrix of the graph, the degree sequence of the graph, and the distances from the starting vertex to each vertex in the graph.

The code consists of two methods for generating and saving the graph as a bitmap and text file, one method for BFS (breadth-first search) traversal of the graph, and some helper methods for calculating the degrees and distances of the graph.

The code uses the iTextSharp library for generating the PDF file and the System.Drawing library for creating the bitmap.

The main method initializes a random generator and prompts the user to enter the number of vertices and the probability of an edge existing between two vertices. It then generates the adjacency matrix of the graph, saves it as a bitmap, saves the graph statistics to the text file, adds the BFS results to the text file, and finally converts the text file to a PDF file.
