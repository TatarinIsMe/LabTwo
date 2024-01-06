using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LAB2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Нахождение экстремума функции - f(x)= 50 - 63x - 25x^2 + x^3 в интервале [-10; 53]");
            Console.WriteLine("Введите необходимое количество генераций");
            var generations = 15;
            try
            {
                generations = int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Число некорректно, берется значение по умолчанию = 15");
            }
            Console.WriteLine();
            var ext = new ExtremumFinder();
            ext.FindExtremum(true,generations); //Максимизация
            ext.FindExtremum(false,generations); //Минимизация
            Console.ReadKey();
        }
    }

    public class DescriptionOutputer
    {
        public void WriteArrayByLine<T>(T[] arr, string text = "")
        {
            Console.WriteLine(text);
            for (int i = 0; i < arr.Length; i++)
                Console.WriteLine(arr[i]);
        }
    }
  
    public class ExtremumFinder
    {
        //Параметры алгоритма
        //Размер популяции - 4, число скрещиваний в популяции - 2, число мутаций - 1 потом на поколение
        //Функция - f(x)= 50 - 63x - 25x^2 + x^3 в интервале [-10; 53]
        string[] population = new string[4];
        string[] crosResult = new string[2];
        int[] interval = new int[64];
        int size = 4;

        DescriptionOutputer outputer = new DescriptionOutputer();
        Random random = new Random();
        double F(int x)
        {
            return 50 - 63 * x - 25 * x * x + x * x * x;
        }
        public ExtremumFinder()
        {
            for (int i = 0; i < interval.Length; i++)
                interval[i] = -10 + i;
            
        }

        public void FindExtremum(bool maximization, int generationCount = 15)
        {
            if (maximization)
                Console.WriteLine("| Нахождения максимума функции |");
            else
                Console.WriteLine("| Нахождения минимума функции |");

            population = InitializeStartPopulation();
            outputer.WriteArrayByLine(population, "...Начальная популяция...");
            string[] decimalPopulation = population;
            population = BinaryConversions.DecimalToBinary(population);
            outputer.WriteArrayByLine(population, "...Начальная популяция в двоичном коде...");
            outputer.WriteArrayByLine(FunctionAssessmentFromDecimal(BinaryConversions.BinaryToDecimalArray(population)), "...Значения функции особей популяции...");
            Console.WriteLine();
            for (int i = 0; i < generationCount; i++)
            {
                Console.WriteLine("| Генерация " + (i + 1) + " |");

                crosResult = PanmixiaCrossover(population);
                outputer.WriteArrayByLine(population, "...Популяция...");
                outputer.WriteArrayByLine(crosResult, "...Потомки...");

                population = Mutation(population, crosResult);
                outputer.WriteArrayByLine(population, "...Популяция с мутацией...");

                if (maximization)
                    population = SelectionMax(population);
                else
                    population = SelectionMin(population);
                outputer.WriteArrayByLine(population, "...Отобранные особи...");

                decimalPopulation = BinaryConversions.BinaryToDecimalArray(population);
                outputer.WriteArrayByLine(decimalPopulation, "...Отобранные особи в десятичном виде...");
                outputer.WriteArrayByLine(FunctionAssessmentFromDecimal(decimalPopulation), "...Значения функции особей популяции...");
                Console.WriteLine();
            }
            double[] yValues = FunctionAssessmentFromDecimal(decimalPopulation);
            double extValue;
            if (maximization)
                extValue = yValues.Max();
            else
                extValue = yValues.Min();
            int yInd = yValues.ToList().IndexOf(extValue);
            outputer.WriteArrayByLine(decimalPopulation, "| Конечная популяция |");
            outputer.WriteArrayByLine(yValues, "| Значения конечной популяции |");
            Console.WriteLine("X = " + decimalPopulation[yInd]);
            Console.WriteLine("Y = " + extValue);
            Console.WriteLine();
        }
        
        public string[] InitializeStartPopulation()
        {
            string[] startPopulation = new string[population.Length];
            for (int i = 0; i < startPopulation.Length; i++)
                startPopulation[i] = random.Next(interval[0],interval[interval.Length-1]).ToString();
            return startPopulation;
        }

        public double[] FunctionAssessmentFromDecimal(string[] popul)
        {
            double[] funcValues = new double[popul.Length];
            try
            {
                for (int i = 0; i < funcValues.Length; i++)
                {
                    funcValues[i] = F(int.Parse(popul[i]));
                }
            }
            catch
            {
                Console.WriteLine("Ошибка при вычислении значений функции популяции!");
            }
            return funcValues;
        }

        public string[] PanmixiaCrossover(string[] population)
        {
            string[] crosResult = new string[this.crosResult.Length];
            for (int i = 0; i < crosResult.Length; i++)
            {
                var id1 = random.Next(0, population.Length);  
                var id2 = random.Next(0, population.Length);
                var parent1 = population[random.Next(0, population.Length)];
                var parent2 = population[random.Next(0, population.Length)];
                while (true)
                {
                    if (id1 == id2)
                        break;
                    id2 = random.Next(0, population.Length);
                    parent2 = population[id2];
                }
                var cross = random.Next(1, parent1.Length);
                crosResult[i] = parent1.Substring(0, cross + 1) + parent2.Substring(cross + 1);
            }
            return crosResult;
        }

        public string[] SelectionMax(string[] population)
        {
            string[] selected;
            selected = population
                    .OrderByDescending(n => F(BinaryConversions.BinaryToDecimal(n)))
                    .Take(size)
                    .ToArray();
            return selected;
        }
        public string[] SelectionMin(string[] population)
        {
            string[] selected;
            selected = population
                    .OrderBy(n => F(BinaryConversions.BinaryToDecimal(n)))
                    .Take(size)
                    .ToArray();
            return selected;
        }

        public string[] Mutation(string[] population, string[] cros)
        {
            string[] unitedPopulation = population.Concat(cros).ToArray();

            var ind = random.Next(unitedPopulation.Length);
            string mutated;
            int pointInd;
            while (true)
            {
                mutated = unitedPopulation[ind];
                pointInd = random.Next(unitedPopulation[ind].Length);
                if (unitedPopulation[ind][pointInd] == '1')
                    mutated = unitedPopulation[ind].Insert(pointInd, "0");
                else
                    mutated = unitedPopulation[ind].Insert(pointInd, "1");
                mutated = mutated.Remove(pointInd + 1, 1);
                var dec = BinaryConversions.BinaryToDecimal(mutated);
                if (dec > interval[0] && dec < interval.Last())
                {
                    unitedPopulation[ind] = mutated;
                    break;
                }
            }
            return unitedPopulation;
        }

    }

    public class BinaryConversions
    {
        public static string[] DecimalToBinary(string[] population)
        {
            string[] binaryPopulation = new string[population.Length];
            for (int i = 0; i < population.Length; i++)
            {
                binaryPopulation[i] = Convert.ToString(int.Parse(population[i]), 2);
                binaryPopulation[i] = StandartizeBinValues(binaryPopulation[i]);
            }
            return binaryPopulation;
        }
        public static string[] BinaryToDecimalArray(string[] population)
        {
            string[] decimalPopul = new string[population.Length];
            for (int i = 0; i < population.Length; i++)
            {
                decimalPopul[i] = BinaryToDecimal(population[i]).ToString();
            }
            return decimalPopul;
        }
        public static int BinaryToDecimal(string bin)
        {
            var dec = Convert.ToInt32(bin, 2);
            if (dec > 53)
                dec -= 128;
            return dec;
        }
        static string StandartizeBinValues(string bin)
        {
            while (bin.Length < 7)
            {
                bin = bin.Insert(0, "0");
            }
            if (bin.Length > 7)
            {
                bin = bin.Substring(bin.Length - 7);
            }
            return bin;
        }
    }
}
