using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using File = System.IO.File;

namespace KMS1_Books_Arbab
{
    public partial class MainWindow : Window
    {
        BackgroundWorker slave = new BackgroundWorker();


        public MainWindow()
        {
            InitializeComponent();
       
        }


        OpenFileDialog openFileDialog = new OpenFileDialog();
        StringBuilder sb = new StringBuilder();
        SaveFileDialog save = new SaveFileDialog();
        char[] delimiter = new char[] { '\0', '\f', '\v','\r', ' ', ',', ':', '\t', '\"', '\n', '{', '}', '[', ']', '=', '/', '-' };


        private void Slave_OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            progressBar.Value = e.ProgressPercentage;
            progressBarString.Text = e.ProgressPercentage.ToString()+"%";
           
        }
        private void UploadButtonOne_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    string path = openFileDialog.FileName;           
                    pathBookOne.Text = path;                          
                    string bookText = File.ReadAllText(path);
                    dataGridBookContentBookOne.DataContext = File.ReadAllText(path);
                   
                    txtCountWordsBookOne.Text = Regex.
                         Split(File.ReadAllText(path), @"[^0-9a-zA-Z]", RegexOptions.IgnorePatternWhitespace)//splitby regex all words ignore spaces
                         .Count()
                         .ToString();
                    txtCountLinesBookOne.Text = File.ReadAllLines(path)
                        .Count()
                        .ToString();
                    CountWordsInBook(path, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



        private void UploadButtonTwo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    string path = openFileDialog.FileName;
        
                    pathBookTwo.Text = path;
                    dataGridBookContentBookTwo.DataContext = File.ReadAllText(openFileDialog.FileName);
                    txtCountWordsBookTwo.Text = Regex.
                         Split(File.ReadAllText(path), @"[^0-9a-zA-Z]", RegexOptions.IgnorePatternWhitespace)
                         .Count()
                         .ToString();
                    txtCountLinesBookTwo.Text = File.ReadAllLines(path)
                        .Count()
                        .ToString();
                    CountWordsInBook(path, false);
                    
                }
            }                          
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



        private void CountWordsInBook(string content, bool dataGridSide)
        {
            string rawText = File.ReadAllText(content);
            try {
                var orderedWords = rawText
                .Split(delimiter, StringSplitOptions.RemoveEmptyEntries) //uses array of delimiters to separate
                .GroupBy(x=>x) 
                .Select(x => new
                {
                    Word = x.Key,
                    Count = x.Count()
                })//assigns props
                .OrderByDescending(x => x.Count) //so that 20 most used are up
                .Take(20);//take 20 most used
                if (dataGridSide == true)
                {
                    dataGridBookContentBookOne.ItemsSource = orderedWords;
                }
                else if (dataGridSide == false)
                {
                    dataGridBookContentBookTwo.ItemsSource = orderedWords;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }



        private void CompareBooks(object sender, RoutedEventArgs e)
        {
            try
            {
                string bookOneUnseparated = File.ReadAllText(pathBookOne.Text);
                string bookTwoUnseparated = File.ReadAllText(pathBookTwo.Text);
                var bookOneSeparated = bookOneUnseparated.Split(delimiter,StringSplitOptions.RemoveEmptyEntries).GroupBy(x => x)
                        .Select(x => new
                        {
                            x.Key,
                        }).ToList();
                var bookTwoSeparated = bookTwoUnseparated.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).GroupBy(x => x).Select(x => new
                {
                    x.Key,
                }).ToList();
                match.Text = MatchesMalone(bookOneSeparated, bookTwoSeparated).ToString("P",CultureInfo.InvariantCulture);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }


        public double MatchesMalone<T>(List<T> lista, List<T> listb)
        {
           
            slave.WorkerReportsProgress = true;
            slave.DoWork += Slave_DoWork;
            slave.ProgressChanged += Slave_OnProgressChanged;
            slave.RunWorkerAsync(progressBar);
            int count = listb.Count;
            int matches = 0;
            foreach (var phrase in listb)
            {
                if (lista.Contains(phrase))
                    matches++;
            }
             return  (double)matches / (double)count;
        }

        private void Slave_DoWork(object sender, DoWorkEventArgs e)
        {
            var slave = sender as BackgroundWorker;
            slave?.ReportProgress(0);
            for (int i = 0; i < 100; i++)
            {
                slave?.ReportProgress(i);
            }
            slave?.ReportProgress(100, "Done iterating");
        }

      

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if(match.Text != "")
                {
                    save.Filter = "Text files|*.txt;|All files|*.*";
                    save.ShowDialog();
                    StreamWriter write = new StreamWriter(save.FileName);
                    string[] pathSplittedOne = pathBookOne.Text.Split(@"\");
                    string bookNameOne = pathSplittedOne[^1];//last index of array is file name
                    string[] pathSplittedTwo = pathBookTwo.Text.Split(@"\");
                    string bookNameTwo = pathSplittedTwo[^1];
                    sb.Append($"Book 1:  {bookNameOne}\nBook 2:  {bookNameTwo} \nBook one matches book two in " + match.Text);
                    write.WriteLine(sb.ToString());
                    write.Close();
                }
                else
                {
                    MessageBox.Show("Please Compare first!");
                }
             
            }
            catch (Exception ex) 
            { 
                MessageBox.Show(ex.Message);
            }
            
        }


    }

}