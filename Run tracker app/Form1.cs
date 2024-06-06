using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Run_tracker_app
{
    public partial class Form1 : Form
    {
        private List<Run> runs = new List<Run>();
        private string filePath = "data.txt";

        public Form1()
        {
            InitializeComponent();
            LoadData();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView.View = View.Details;
            listView.Columns.Add("Date", 94);
            listView.Columns.Add("Distance (KM)", 95);
            listView.Columns.Add("Time (MM:SS)", 95);
            listView.Columns.Add("Pace (min/km)", 95);
            listView.Columns.Add("Avg Speed (Kph)", 95);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            // Input Validation
            bool validDistance = double.TryParse(txtDistance.Text, out double distance);
            bool validTime = TimeSpan.TryParseExact(txtTime.Text, "mm\\:ss", null, out TimeSpan time);

            if (!validDistance || distance < 0 || distance >= 100)
            {
                MessageBox.Show("Please enter a valid distance in the format (00.00)");
            }
            else if (!validTime)
            {
                MessageBox.Show("Please enter a valid time in the format (MM:SS)");
            }
            else
            {
                // Objects and add Runs
                Run runObject = new Run();
                runObject.date = datePicker.Value;
                runObject.distance = double.Parse(txtDistance.Text);
                runObject.time = time;

                double pace = time.TotalMinutes / distance;
                double avgspeed = distance / time.TotalHours;

                runObject.pace = pace;
                runObject.avgSpeed = avgspeed;

                runs.Add(runObject);
                AddToListView(runObject);

                // Clear Fields
                datePicker.Value = DateTime.Now;
                txtDistance.Text = "";
                txtTime.Text = "";

                // Confirmation MB
                MessageBox.Show("Run Successfully Logged");

                // Update overall stats and save data
                UpdateOverallStats();
                SaveData();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveData();
        }

        private void LoadData()
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(';');
                    if (parts.Length == 5)
                    {
                        DateTime date = DateTime.Parse(parts[0]);
                        double distance = double.Parse(parts[1]);
                        TimeSpan time = TimeSpan.Parse(parts[2]);
                        double pace = double.Parse(parts[3]);
                        double avgSpeed = double.Parse(parts[4]);

                        Run runObject = new Run
                        {
                            date = date,
                            distance = distance,
                            time = time,
                            pace = pace,
                            avgSpeed = avgSpeed
                        };

                        runs.Add(runObject);
                        AddToListView(runObject);
                    }
                }

                UpdateOverallStats();
            }
        }

        private void SaveData()
        {
            List<string> lines = new List<string>();
            foreach (Run run in runs)
            {
                string line = $"{run.date};{run.distance};{run.time};{run.pace};{run.avgSpeed}";
                lines.Add(line);
            }
            File.WriteAllLines(filePath, lines);
        }

        private void AddToListView(Run run)
        {
            ListViewItem listItem = new ListViewItem(run.date.ToShortDateString());
            listItem.SubItems.Add(run.distance.ToString());
            listItem.SubItems.Add(run.time.ToString("mm\\:ss"));
            listItem.SubItems.Add(TimeSpan.FromMinutes(run.pace).ToString("mm\\:ss"));
            listItem.SubItems.Add(run.avgSpeed.ToString("0.00"));
            listView.Items.Add(listItem);
        }

        private void UpdateOverallStats()
        {
            double totaldistance = runs.Sum(run => run.distance);
            int totalruns = runs.Count;

            if (totalruns > 0)
            {
                double totalavgpace = runs.Sum(run => run.pace);
                double tavgpace = totalavgpace / totalruns;
                TimeSpan ttavg = TimeSpan.FromMinutes(tavgpace);
                lblTotalAvgPace.Text = ttavg.ToString("mm\\:ss");

                double fastestPace = runs.Min(run => run.pace);
                TimeSpan fastestpacets = TimeSpan.FromMinutes(fastestPace);
                lblBestPace.Text = fastestpacets.ToString("mm\\:ss");
            }
            else
            {
                lblTotalAvgPace.Text = "00:00";
                lblBestPace.Text = "00:00";
            }

            lblTotalDistance.Text = totaldistance.ToString();
            lblTotalRuns.Text = totalruns.ToString();
        }

        public class Run
        {
            public DateTime date { get; set; }
            public double distance { get; set; }
            public TimeSpan time { get; set; }
            public double pace { get; set; }
            public double avgSpeed { get; set; }
        }
    }
}

