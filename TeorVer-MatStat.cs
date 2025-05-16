using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace ЛАБА_ТВИМС__1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            chartForHitogramms.ChartAreas[0].AxisX.LabelStyle.Format = "F2";
        }

        Random rand = new Random();

        private double Max;
        private double Min;
        private int initNumOfValuesReg = 0;
        private int initNumOfValuesNorm = 0;
        private int initNumOfValuesLinVal = 0;
        private double[] regularDistrib;
        private double[] normalDistrib;
        double[] Yvalue;
        double[] Xvalue;
        double[] sample;
        double[] xi;
        double sampleVar, groupedMedian, orderedMedian, modeSimple, modeAdjusted, avg;

        private void button1_Click(object sender, EventArgs e)
        {
            int numOfvalues = (int)this.numericUpDown1.Value;
            int numOfBars = (int)this.numericUpDown2.Value;

            if (CheckForNewParametersReg(numOfvalues) || regularDistrib == null)
                regularDistrib = CreateArrayForRegularDistib(numOfvalues);

            ShowChart(regularDistrib, numOfvalues, numOfBars);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            int numOfvalues = (int)this.numericUpDown1.Value;
            int numOfBars = (int)this.numericUpDown2.Value;

            if (CheckForNewParametersNorm(numOfvalues) || normalDistrib == null)
                normalDistrib = CreateArrayForNormalDistrib(numOfvalues);

            ShowChart(normalDistrib, numOfvalues, numOfBars);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            regularDistrib = null;
            regularDistrib = CreateArrayForRegularDistib((int)this.numericUpDown1.Value);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            normalDistrib = null;
            normalDistrib = CreateArrayForNormalDistrib((int)this.numericUpDown1.Value);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            int numOfvalues = (int)this.numericUpDown1.Value;
            int numOfBars = (int)this.numericUpDown2.Value;

            sample = null;
            xi = null;

            xi = CreateBordersForLin(numOfBars);
            sample = CreateSampleForLin(numOfvalues, numOfBars);
            sampleVar = ComputeSampleVariance();
            groupedMedian = ComputeGroupedMedian(numOfvalues, numOfBars);
            orderedMedian = ComputeOrderedMedian();
            var modes = ComputeModes();
            modeSimple = modes.modeSimple;
            modeAdjusted = modes.modeAdjusted;
            avg = sample.Average();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            chartForHitogramms.Series[0].Points.Clear();

            int numOfvalues = (int)numericUpDown1.Value;
            int numOfBars = (int)numericUpDown2.Value;
            int numOfBarsForHist = (int)numericUpDown3.Value;

            int numVFS = initNumOfValuesLinVal;

            xi = CreateBordersForLin(numOfBars);

            if (CheckForNewParametersLin(numOfvalues))
                sample = CreateSampleForLin(numOfvalues, numOfBars);

            ShowChart(sample, numOfvalues, numOfBarsForHist);

            numVFS = initNumOfValuesLinVal;

            sampleVar = ComputeSampleVariance();
            groupedMedian = ComputeGroupedMedian(numOfvalues, numOfBars);
            orderedMedian = ComputeOrderedMedian();
            var modes = ComputeModes();
            modeSimple = modes.modeSimple;
            modeAdjusted = modes.modeAdjusted;
            avg = sample.Average();

            string result =
                "Результаты оценки распределения:\n\n" +
                $"1. Дисперсия:\n" +
                $"   - По исходной (несгруппированной) выборке: {sampleVar:F4}\n\n" +
                $"2. Медиана:\n" +
                $"   - Через интервальный ряд (кумулятивная кривая): {groupedMedian:F4}\n" +
                $"   - По упорядоченной выборке: {orderedMedian:F4}\n\n" +
                $"3. Мода:\n" +
                $"   - Середина интервала с наибольшей частотой: {modeSimple:F4}\n" +
                $"   - С поправкой на соседние интервалы: {modeAdjusted:F4}\n\n" +
                $"4. Среднее значение выборки: {avg:F4}";

            this.label4.Text = result;
        }
        private double[] CreateBordersForLin(int numOfBars)
        {
            double[] xi = null;
            xi = new double[numOfBars + 1];
            for (int i = 0; i <= numOfBars; i++)
                xi[i] = 2 * Math.Sqrt((double)i / numOfBars); // xi = 2√(i/n)
            return xi;
        }
        private double[] CreateSampleForLin(int numOfvalues, int numOfBars)
        {
            double[] sample_h = new double[numOfvalues];
            double Pi = 1 / (double)numOfBars;

            for (int i = 0; i < numOfvalues; i++)
            {
                double x = GetX(Pi, xi);
                sample_h[i] = x;
            }
            Array.Sort(sample_h);

            return sample_h;
        }
        private double GetX(double Pi, double[] xi)
        {
            int idx = 1;
            double r = rand.NextDouble();

            while (true)
            {
                if (r < Pi) break;
                r = r - Pi;
                idx++;
            }

            double r2 = rand.NextDouble();
            double x = xi[idx - 1] + r2 * (xi[idx] - xi[idx - 1]);
            return x;
        }
        private double ComputeSampleVariance()
        {
            double mean = sample.Average();
            double sum = 0;
            for (int i = 0; i < sample.Length; i++)
                sum += Math.Pow(sample[i] - mean, 2);
            double variance = (1 / (double)(sample.Length - 1) * sum);
            return variance;
        }
        private double ComputeGroupedMedian(int numValues, int numOfBars)
        {
            int bars = Yvalue.Length;
            double cumulative = 0.0;
            for (int i = 0; i < bars; i++)
            {
                double prevCumulative = cumulative;
                double width = i != bars - 1 ? Xvalue[i + 1] - Xvalue[i] : 2 - Xvalue[i];

                cumulative += Yvalue[i] * width;
                if (cumulative >= 0.5)
                {
                    double L = Xvalue[i];  
                    double h = width;      
                    double median = L + ((0.5 - prevCumulative) / Yvalue[i]) * h;
                    return median;
                }
            }
            return -1; // На практике эта точка найдется, поэтому возвращение -1 означает ошибку.
        }
        private double ComputeOrderedMedian()
        {
            int n = sample.Length;
            if (n % 2 == 1)
                return sample[n / 2];
            else
                return (sample[n / 2 - 1] + sample[n / 2]) / 2.0;
        }
        private (double modeSimple, double modeAdjusted) ComputeModes()
        {
            int bars = Yvalue.Length;
            int modalIndex = 0;
            double maxCount = Yvalue[0];
            for (int i = 1; i < bars; i++)
            {
                if (Yvalue[i] > maxCount)
                {
                    maxCount = Yvalue[i];
                    modalIndex = i;
                }
            }
            double modeSimple = (Xvalue[modalIndex] + (modalIndex != bars - 1 ? Xvalue[modalIndex + 1] : 2)) / 2.0;

            double L = Xvalue[modalIndex];
            double h = (modalIndex != bars - 1 ? Xvalue[modalIndex + 1] : 2) - Xvalue[modalIndex];

            double f_m = Yvalue[modalIndex];
            double f_prev = modalIndex == 0 ? 0 : Yvalue[modalIndex - 1];
            double f_next = modalIndex == bars - 1 ? 0 : Yvalue[modalIndex + 1];
            double denominator = (f_m - f_prev) + (f_m - f_next);
            double modeAdjusted = denominator == 0 ? modeSimple : L + ((f_m - f_prev) / denominator) * h;
            return (modeSimple, modeAdjusted);
        }
        private double[] CreateArrayForRegularDistib(int numberOfvalues)
        {
            double[] massive = new double[numberOfvalues];
            Random rand = new Random();

            for (int i = 0; i < numberOfvalues; i++)
                massive[i] = Math.Round(rand.NextDouble(), 3);     // Получение случайного числа в радиусе от 0 до 1

            Array.Sort(massive);
            return massive;
        }
        private double[] CreateArrayForNormalDistrib(int numberOfvalues)
        {
            double[] massive = new double[numberOfvalues];
            double sum;
            Random rand = new Random();

            for (int i = 0; i < numberOfvalues; i++)
            {
                sum = 0;
                for (int j = 0; j < 20; j++)
                    sum += rand.NextDouble();
                sum = Math.Round(sum, 3);
                massive[i] = sum;
            }

            Array.Sort(massive);
            return massive;
        }
        private bool CheckForNewParametersLin(int numOfvalues)
        {
            if (initNumOfValuesLinVal != numOfvalues)
            {
                initNumOfValuesLinVal = numOfvalues;
                return true;
            }
            return false;
        }
        private bool CheckForNewParametersReg(int numOfvalues)
        {
            if (initNumOfValuesReg != numOfvalues)
            {
                initNumOfValuesReg = numOfvalues;
                return true;
            }
            return false;
        }
        private bool CheckForNewParametersNorm(int numOfvalues)
        {
            if (initNumOfValuesNorm != numOfvalues)
            {
                initNumOfValuesNorm = numOfvalues;
                return true;
            }
            return false;
        }
        private void ShowChart(double[] massive, int numOfvalues, int numOfBars)
        {
            chartForHitogramms.Series[0].Points.Clear();

            Min = massive[0];
            Max = massive[numOfvalues - 1];

            double step = (Max - Min) / numOfBars;
            double cur_Step = 0;

            Yvalue = new double[numOfBars];
            Xvalue = new double[numOfBars];

            int idx = 0;

            for (int i = 0; i < numOfBars; i++)
            {
                double lowborder = Min + i * step;
                double highborder = Min + (i + 1) * step;

                while (idx < numOfvalues)
                {
                    if (massive[idx] >= lowborder && massive[idx] < highborder)
                    {
                        Yvalue[i]++;
                        idx++;
                    }
                    else break;
                }
                Yvalue[i] = Math.Round(Yvalue[i] / (numOfvalues * step), 2);
            }

            for (int i = 0; i < numOfBars; i++)
            {
                cur_Step += i == 0 ? 0 : step;
                Xvalue[i] = cur_Step;
                double lowVal = Math.Round(cur_Step, 2);
                double highVal = Math.Round(cur_Step + step, 2);
                chartForHitogramms.Series[0].Points.AddXY($"{lowVal} - {highVal}", Yvalue[i]);
            }
        }
    }
}
