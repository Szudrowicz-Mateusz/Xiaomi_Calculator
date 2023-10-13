using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

/*
    Creator: Mateusz Szudrowicz
    Version: .NET 6.0
    It's a copy of Xiaomi calculator.
    
    Functions named Name_Function are fuctions to click from XAML
    Functions named NameFunction are function adding logic to the program

    The rest text is in the git repository https://github.com/Szudrowicz-Mateusz/Xiaomi_Calculator
 */

namespace CalcualtorCS_Z_Lista
{
    [Serializable]
    public class CalculationItem
    {
        public string Expression { get; set; }
        public double Result { get; set; }

        public override String ToString()
        {
            return $"{Expression} \n= {Result}";
        }
    }


    public class FileStorage
    {
        private string storageDirectory;

        public FileStorage()
        {
            // Get the application's working directory
            string workingDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Create a subdirectory for storing your files, e.g., "CalculationData"
            storageDirectory = Path.Combine(workingDirectory, "CalculationData");

            // Ensure the directory exists, create it if it doesn't
            if (!Directory.Exists(storageDirectory))
            {
                Directory.CreateDirectory(storageDirectory);
            }
        }

        // Save the calculations list to a file
        public void SaveCalculationsToFile(List<CalculationItem> calculations)
        {
            try
            {
                string filePath = Path.Combine(storageDirectory, "calculations.txt");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fileStream, calculations);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., file access errors
                MessageBox.Show("Error saving calculations: " + ex.Message);
            }
        }

        // Load the calculations list from a file
        public List<CalculationItem> LoadCalculationsFromFile()
        {
            List<CalculationItem> calculations = new List<CalculationItem>();

            try
            {
                string filePath = Path.Combine(storageDirectory, "calculations.txt");

                if (File.Exists(filePath))
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        calculations = (List<CalculationItem>)binaryFormatter.Deserialize(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., file access errors or deserialization errors
                MessageBox.Show("Error loading calculations: " + ex.Message);
            }

            return calculations;
        }
    }

    public partial class MainWindow : Window
    {
        /* Variables used for the whole program */
        private String currentInput = String.Empty;
        private String currentNumber = String.Empty;
        private String operation = String.Empty;

        private double result = 0;
        private double saved_result = 0;

        private FileStorage fileStorage;

        private List<CalculationItem> calculations = new List<CalculationItem>();


        public MainWindow()
        {
            InitializeComponent();

            DataContext = this; // Set DataContext to this MainWindow instance

            fileStorage = new FileStorage();

            // Load calculations from the file, if any
            calculations = fileStorage.LoadCalculationsFromFile();
            UpdateListBox(currentInput, result);

            // Subscribe to the Closing event to save calculations when the application is closed
            Closing += MainWindow_Closing;
        }

        /* Saving to File */ 
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save calculations to the file when the application is closed
            fileStorage.SaveCalculationsToFile(calculations);
        }

        /* Helping Function */
        private char GetChar(ref object sender) // Functions to access digit or number from button clicks
        {
            string full_command = sender?.ToString() ?? string.Empty; // Default to an empty string if sender is null
            return full_command[full_command.Length - 1];
        }


        /* Displaying */
        private void UpdateTextBox(string expression, double result)
        {
            inputTextBox.Text = $"{expression}\n= {result}";
        }

        private void UpdateListBox(string expression, double result)
        {

            if (expression != String.Empty){
                CalculationItem newItem = new CalculationItem { Expression = expression, Result = result };
                calculations.Add(newItem);

                // Scroll to the last item to keep the scroll bar at the bottom
                calculationListBox.ScrollIntoView(newItem);
            }
                

            // Update the ListBox
            calculationListBox.ItemsSource = null;
            calculationListBox.ItemsSource = calculations;

        }


        /* Clearing and Deleting */
        private void Clear_Click(object sender, RoutedEventArgs e) // It depends if numbers are in our textbox or not
        {
            if (Clear.Content.ToString() == "C")
            {
                ResetVariables();
                Clear.Content = "AC";
            }
            else if (Clear.Content.ToString() == "AC")
            {
                ResetVariables();
                ClearCalculations();
            }

            UpdateTextBox(currentInput, result);
        }

        public void Delete_Click(object sender, RoutedEventArgs e) // Delete but It's only working on the digit before we choose our sing
        {
            if (currentNumber == String.Empty)
                return;

            currentNumber = currentNumber.Remove(currentNumber.Length - 1);
            currentInput = currentInput.Remove(currentInput.Length - 1);

            UpdateTextBox(currentInput, result);
        }
        private void ResetVariables() // Reseting Variables
        {
            currentInput = String.Empty;
            operation = String.Empty;
            currentNumber = String.Empty;

            result = 0;
            saved_result = 0;
        }

        private void ClearCalculations() // Clearing the scrolled window from records
        {
            calculations.Clear();
            calculationListBox.ItemsSource = null;
            calculationListBox.Items.Clear(); // Clear the ListBox items
        }

        private void CheckClear()
        {
            if (currentInput == String.Empty)
                Clear.Content = "AC";
            else
                Clear.Content = "C";
        }

        /* Numbers Clicking and Doing Calculations */
        public void Number_Click(object sender, RoutedEventArgs e)
        {
            char number = GetChar(ref sender);
            String input = String.Empty;

            switch (number)
            {
                case '0':
                    input += "0";
                    break;
                case '1':
                    input += "1";
                    break;
                case '2':
                    input += "2";
                    break;
                case '3':
                    input += "3";
                    break;
                case '4':
                    input += "4";
                    break;
                case '5':
                    input += "5";
                    break;
                case '6':
                    input += "6";
                    break;
                case '7':
                    input += "7";
                    break;
                case '8':
                    input += "8";
                    break;
                case '9':
                    input += "9";
                    break;
                default:
                    MessageBox.Show("This number is unrecognized");
                    break;
            }

            if (currentNumber == null)
                return;

            currentNumber += input;
            currentInput += input;

            UpdateResult();
        }

        private void UpdateResult()
        {
            // Checking if we have any operation to do
            if (operation != String.Empty)
                DoOperation();
            else
                result = Convert.ToDouble(currentNumber);

            UpdateTextBox(currentInput, result); // Call the method to update the inputTextBox

            CheckClear();
        }

        private void DoOperation()
        {

            double second_digit = Convert.ToDouble(currentNumber);
            result = saved_result;

            switch (operation)
            {
                case "+":
                    result += second_digit;
                    break;
                case "-":
                    result -= second_digit;
                    break;
                case "*":
                    result *= second_digit;
                    break;
                case "/":
                    result /= second_digit;
                    break;
                case "%":
                    result = result * (second_digit / 100);
                    break;
            }
        }

        /* Signs Clicking */

        private void Sign_Click(object sender, RoutedEventArgs e) // We are saving here our operations to do it after the user input next number
        {
            if (currentNumber == String.Empty)
                return;

            char sign = GetChar(ref sender);

            switch (sign)
            {
                case '+':
                    operation = "+";
                    currentInput += " + ";
                    break;
                case '-':
                    operation = "-";
                    currentInput += " - ";
                    break;
                case '*':
                    operation = "*";
                    currentInput += " * ";
                    break;
                case '/':
                    operation = "/";
                    currentInput += " / ";
                    break;
                case '%':
                    operation = "%";
                    currentInput += " % ";
                    break;
            }

            currentNumber = String.Empty;
            saved_result = result;

            UpdateTextBox(currentInput, result); // Call the method to update the inputTextBox
        }

        public void Equals_Click(object sender, RoutedEventArgs e)
        {
            UpdateListBox(currentInput, result);

            ResetVariables();

            inputTextBox.Text = currentInput;
        }

        public void Coma_Click(object sender, RoutedEventArgs e)
        {
            if (currentNumber.Contains(','))
                return;

            currentNumber += ",";
            currentInput += ",";

            UpdateTextBox(currentInput, result);
        }

        public void Turn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("In Xiaomi it's an turn button but I don't offer sucha a feature so \n YOU HAVE TO BE HAPPY YOU SEE THIS AMAZING MESSAGE BOX");
        }

        private void calculationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e){} //Needed function to run this app

    }
}
