using System;
using System.Collections.Generic;
using System.Linq;
using static MongoDB.Driver.WriteConcern;
using System.Reflection.Emit;

namespace ARTANET__demo_
{
    public static class Graphs
    {
        public static string graphType(string graphType, string[] x, double[] y)
        {
            string rLabels = string.Join(",", x);
            string rValues = string.Join(",", y.Select(y => $"'{y}'"));

            if (x.Length != y.Length)
            {
                throw new ArgumentException("Number of values must match the number of labels.");
            }

            string rScript;
            // Bar Graph
            if (graphType == "Bar")
            {
                rScript = $@"
                    # Load necessary libraries
                    library(ggplot2)

                    # Create a data frame from the data
                    df <- data.frame(labels = c({rLabels}), values = c({rValues}))

                    # Create a bar plot
                    p <- ggplot(df, aes(x = x, y = y)) +
                         geom_bar(stat = 'identity') +
                         ggtitle('{"Bar Graph"}')

                    # Print the plot
                    print(p)
                ";
            }
            // Pie Chart
            else if (graphType == "Pie")
            {
                // Generate R script for pie chart
                rScript = $@"
                    # Load necessary libraries
                    library(ggplot2)

                    # Create a data frame from the data
                    df <- data.frame(values = c({rValues}), labels = c({rLabels}))

                    # Create a pie chart
                    p <- ggplot(df, aes(x = '', y = values, fill = labels)) +
                         geom_bar(stat = 'identity', width = 1) +
                         coord_polar(theta = 'y') +
                         ggtitle('{"Pie Chart"}') +
                         theme_void()

                    # Print the plot
                    print(p)
                ";
            }
            // Scatter Plot
            else if (graphType == "Scatter")
            {
                // Create the scatter plot
                rScript = $@"
                    # Load necessary libraries
                    library(ggplot2)
                
                    # Create a data frame from the data
                    df <- data.frame(values = c({rValues}), labels = c({rLabels}))
                
                    # Create a plot
                    p <- ggplot(df, aes(x = x, y = y)) +
                         geom_point() +
                         ggtitle('Scatter Plot')
                
                    # Print the plot
                    print(p)
                ";
            }
            // Heat Map
            else if (graphType == "Heat")
            {
                // Determine the dimensions of the matrix
                int rows = (int)Math.Sqrt(rValues.Length);
                int cols = (int)Math.Ceiling((double)rValues.Length / rows);

                // Create the matrix
                double[,] matrix = new double[rows, cols];
                int index = 0;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (index < rValues.Length)
                        {
                            matrix[i, j] = rValues[index];
                            index++;
                        }
                        else
                        {
                            // If there are fewer values than matrix cells, fill the remaining cells with 0
                            matrix[i, j] = 0;
                        }
                    }
                }

                int rRows = matrix.GetLength(0);
                int rCols = matrix.GetLength(1);
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Console.Write(matrix[i, j] + "\t");
                    }
                    Console.WriteLine();
                }

                // Create the heatmap
                rScript = $@"
                    # Load necessary libraries
                    library(ggplot2)
                
                    # Convert the matrix to a data frame
                    df <- as.data.frame(matrix)
                
                    # Create a plot
                    p <- ggplot(df, aes(x = X1, y = X2, fill = value)) +
                         geom_tile() +
                         scale_fill_gradient(low = 'white', high = 'blue') +
                         ggtitle('Heatmap')
                
                    # Print the plot
                    print(p)
                ";
            }
            else
            {
                rScript = string.Empty;
            }
            return rScript;
        }
    }
}
